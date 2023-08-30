using System.Diagnostics;
using System.Text.Json;
using System.Web;
using Force.Crc32;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Services;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using WebMarkupMin.Core;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Global build manager
/// </summary>
public sealed class BuildManager
{
    private readonly ILogger<BuildManager> logger;
    private readonly HtmlMinifier htmlMinifier;
    private readonly HtmlHighlightService highlightService;
    private readonly GoogleStorageService storageService;
    private readonly VoltProjectsBuilderConfig config;
    private readonly Dictionary<string, Builder> builders;

    private static string[] supportedLangs = new[] { "csharp" };

    /// <summary>
    ///     Creates a new <see cref="BuildManager"/> instance
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="htmlMinifier"></param>
    /// <param name="highlightService"></param>
    /// <param name="storageService"></param>
    /// <param name="config"></param>
    /// <param name="serviceProvider"></param>
    public BuildManager(
        ILogger<BuildManager> logger,
        HtmlMinifier htmlMinifier,
        HtmlHighlightService highlightService,
        GoogleStorageService storageService,
        IOptions<VoltProjectsBuilderConfig> config,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.htmlMinifier = htmlMinifier;
        this.highlightService = highlightService;
        this.storageService = storageService;
        this.config = config.Value;
        builders = new Dictionary<string, Builder>();
        IEnumerable<Type> foundBuilders = ReflectionHelper.GetInheritedTypes<Builder>();

        //Create all builders
        foreach (Type foundBuilder in foundBuilders)
        {
            BuilderNameAttribute attribute = (BuilderNameAttribute)Attribute.GetCustomAttribute(foundBuilder, typeof(BuilderNameAttribute))!;

            Builder builder = (Builder)ActivatorUtilities.CreateInstance(serviceProvider, foundBuilder);
            builders.Add(attribute.Name, builder);
            this.logger.LogDebug("Created builder {Builder}", attribute.Name);
        }
    }

    /// <summary>
    ///     Builds a select project
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectVersion"></param>
    /// <param name="projectPath"></param>
    public async Task BuildProject(VoltProjectDbContext dbContext, ProjectVersion projectVersion, string projectPath, CancellationToken cancellationToken)
    {
        //First, get the builder
        KeyValuePair<string, Builder>? buildFindResult = builders.FirstOrDefault(x => x.Key == projectVersion.DocBuilderId);
        if (buildFindResult == null)
            throw new NotImplementedException($"Builder {projectVersion.DocBuilderId} was not found!");

        //We have a builder
        Builder builder = buildFindResult.Value.Value;

        //Check that docs exist
        string docsLocation = Path.Combine(projectPath, projectVersion.DocsPath);
        if (!Directory.Exists(docsLocation))
            throw new DirectoryNotFoundException("Failed to find docs dir!");
        
        //We will check this after the build
        string builtDocsLocation = Path.Combine(projectPath, projectVersion.DocsBuiltPath);

        //First, run the build process
        ExecuteDocBuilderProcess(builder, projectVersion, docsLocation, builtDocsLocation);
        
        //Now check that built docs exist
        if (!Directory.Exists(builtDocsLocation))
            throw new DirectoryNotFoundException("Failed to find built docs dir!");

        //Build models
        BuildResult buildResult = builder.BuildProject(projectVersion, docsLocation, builtDocsLocation);
        
        //Upsert results into DB
        //Upsert project menu
        ProjectMenu projectMenu = buildResult.ProjectMenu;
        string json = JsonSerializer.Serialize(projectMenu.LinkItem);
        await dbContext.Database
            .ExecuteSqlAsync(
                $"INSERT INTO public.project_menu (project_version_id, last_update_time, link_item) VALUES ({projectMenu.ProjectVersionId}, {projectMenu.LastUpdateTime}, {json}::jsonb) ON CONFLICT (project_version_id) DO UPDATE SET last_update_time = EXCLUDED.last_update_time, link_item = EXCLUDED.link_item RETURNING *;", cancellationToken);
        
        //Upsert project TOCs
        ProjectToc[] tocItems = buildResult.ProjectTocs;
        
        int tocIndex = 1;
        object[] tocParams = new object[1 + tocItems.Length * 2];
        tocParams[0] = projectVersion.Id;
        string tocParamsPlaceholder = string.Join(",", tocItems.Select(x =>
        {
            tocParams[tocIndex] = x.TocRel;
            tocParams[tocIndex + 1] = JsonSerializer.Serialize(x.TocItem);
               
            return $"ROW(@p{tocIndex++}, @p{tocIndex++}::jsonb)";
        }));
        
        //Upset project TOCs
        tocItems = await dbContext.ProjectTocs
            .FromSqlRaw($"SELECT * FROM public.upsert_project_tocs(@p0, ARRAY[{tocParamsPlaceholder}]::upsertedtoc[]);", tocParams)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        //Pre-Process pages
        ProjectPage[] pages = buildResult.ProjectPages;
        await Parallel.ForEachAsync(pages, cancellationToken, async (page, token) =>
        {
            string pageCurrentPath = Path.Combine(builtDocsLocation, page.Path);
            
            HtmlDocument doc = new();
            doc.LoadHtml(page.Content);
            
            //Get first p block to get page description from
            HtmlNode? node = doc.DocumentNode.SelectSingleNode("//p[not(*)]");
            page.Description = node?.InnerText ?? page.Title;

            //Parse code blocks
            HtmlNodeCollection? codeBlocks = doc.DocumentNode.SelectNodes("//pre/code");
            if (codeBlocks != null)
            {
                foreach (HtmlNode codeBlock in codeBlocks)
                {
                    string? text = codeBlock.InnerHtml;
                    string? language = null;

                    //Try to pick-up on the language
                    HtmlAttribute? languageAttribute = codeBlock.Attributes["class"];
                    if (languageAttribute != null)
                    {
                        language = languageAttribute.Value.Replace("lang-", "");
                        if (!supportedLangs.Contains(language))
                            language = null; //use autodetect
                    }

                    if (text != null)
                    {
                        string parsedCodeBlock =
                            highlightService.ParseCodeBlock(HttpUtility.HtmlDecode(text), language);
                        codeBlock.InnerHtml = parsedCodeBlock;
                    }

                    codeBlock.SetAttributeValue("class", "hljs shadow");
                }
            }
            
            //Parse Images
            //TODO: Perhaps optimize this xpath to only include elements with src attribute
            HtmlNodeCollection? images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (HtmlNode imageNode in images)
                {
                    HtmlAttribute? srcAttribute = imageNode.Attributes["src"];
                    
                    //Src attribute is invalid, weird
                    if (srcAttribute == null)
                    {
                        logger.LogWarning("Image found on page {PageTitle} doesn't have a src attribute!", page.Title);
                        continue;
                    }
                    
                    //Get image file
                    string imageSrc = srcAttribute.Value;
                    
                    //Off-Site Image, don't care about it
                    if(imageSrc.StartsWith("http"))
                        continue;

                    string imagePath = Path.Combine(pageCurrentPath, imageSrc);
                    if (!File.Exists(imagePath))
                    {
                        logger.LogWarning("Could not found image on page {PageTitle} at location {Path}!", page.Title, imagePath);
                        continue;
                    }

                    string imagePathInProject = Path.GetRelativePath(builtDocsLocation, imagePath);
                    string fileExtension = Path.GetExtension(imagePathInProject);

                    try
                    {
                        //We have the image file, now pre-process it and upload to online storage
                        Image image = await Image.LoadAsync(imagePath, token);

                        MemoryStream imageMemoryStream = new();
                        await image.SaveAsWebpAsync(imageMemoryStream, token);
                        image.Dispose();
                        
                        imageMemoryStream.Position = 0;
                        
                        //Path in storage service that the file should live at
                        string path = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag,
                            $"{imagePathInProject[..^fileExtension.Length]}.webp");
                        
                        //Try to get file
                        uint? existingFileHash = await storageService.GetFileHashAsync(path, token);
                        string filePublicUrl;
                        if (existingFileHash != null)
                        {
                            //TODO: Looks like .NET 8 seems to have some CRC32C hash methods
                            Crc32CAlgorithm crc = new();
                            byte[] hash = await crc.ComputeHashAsync(imageMemoryStream, token);
                            crc.Dispose();
                            
                            uint crc32 = BitConverter.ToUInt32(hash);

                            //Compare hashes, if they are the same, leave it alone, otherwise, re-upload
                            if (crc32 != existingFileHash)
                            {
                                logger.LogDebug("Storage file {File} hashes were not the same, re-uploading...", path);
                                filePublicUrl = await storageService.UploadFileAsync(imageMemoryStream, path, "image/webp", token);
                            }
                            else
                            {
                                logger.LogDebug("Storage file {File} hashes are still the same. Not touching...", path);
                                filePublicUrl = Path.Combine(config.StorageConfig.PublicUrl, path);
                            }
                        }
                        else
                        {
                            //Upload image to storage
                            logger.LogDebug("Uploading file {File} to storage...", path);
                            filePublicUrl = await storageService.UploadFileAsync(imageMemoryStream, path, "image/webp", token);
                        }
                        
                        imageNode.SetAttributeValue("src", filePublicUrl);
                        
                        //Cleanup
                        await imageMemoryStream.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occured while processing an image on page {PageTitle}!", page.Title);
                    }
                }
            }

            //New HTML Content
            string content = doc.DocumentNode.OuterHtml;

            //Minify HTML
            page.Content = htmlMinifier.Minify(content).MinifiedContent;

            //Handle TOC
            if (page.ProjectToc != null)
            {
                ProjectToc? toc = tocItems.FirstOrDefault(x => x.TocRel == page.ProjectToc.TocRel);
                page.ProjectTocId = toc.Id;
                page.ProjectToc = null;
            }
        });
        
        //Upsert pages
        int pageIndex = 1;
        int pageItemsCount = 11;
        object?[] pageParams = new object[1 + pages.Length * pageItemsCount];
        pageParams[0] = projectVersion.Id;
        string pageParamsPlaceholder = string.Join(",", pages.Select(x =>
        {
            pageParams[pageIndex] = x.PublishedDate;
            pageParams[pageIndex + 1] = x.Title;
            pageParams[pageIndex + 2] = x.TitleDisplay;
            pageParams[pageIndex + 3] = x.GitUrl;
            pageParams[pageIndex + 4] = x.Aside;
            pageParams[pageIndex + 5] = x.WordCount;
            pageParams[pageIndex + 6] = x.ProjectTocId;
            pageParams[pageIndex + 7] = x.TocRel;
            pageParams[pageIndex + 8] = x.Path;
            pageParams[pageIndex + 9] = x.Description;
            pageParams[pageIndex + 10] = x.Content;

            return $"ROW(@p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++})";
        }));

        //Upsert project pages
        //No return needed on this one, so we will use dbContext.Database
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SELECT public.upsert_project_pages(@p0, ARRAY[{pageParamsPlaceholder}]::upsertedpage[]);", pageParams);
    }

    private void ExecuteDocBuilderProcess(Builder builder, ProjectVersion projectVersion, string docsLocation, string docsBuiltLocation)
    {
        DocBuilder docBuilder = projectVersion.DocBuilder;

        string[]? arguments = docBuilder.Arguments;
        builder.PrepareBuilder(ref arguments, docsLocation, docsBuiltLocation);

        arguments ??= Array.Empty<string>();

        ProcessStartInfo processStartInfo = new ProcessStartInfo(docBuilder.Application)
        {
            Arguments = string.Join(' ', arguments),
            WorkingDirectory = docsLocation
        };
        
        //TODO: Better way of doing this, please...
        if(docBuilder.EnvironmentVariables != null)
            foreach (string environmentVariable in docBuilder.EnvironmentVariables)
            {
                string[] splitEnv = environmentVariable.Split('=');
                if (splitEnv.Length != 2)
                    throw new ArgumentOutOfRangeException("Doc build environment variables do not equal to two split!");

                processStartInfo.EnvironmentVariables[splitEnv[0]] = splitEnv[1];
            }

        Process process = new();
        process.StartInfo = processStartInfo;
        process.Start();

        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception("Docfx failed to exit cleanly!");
        
        process.Dispose();
    }
}