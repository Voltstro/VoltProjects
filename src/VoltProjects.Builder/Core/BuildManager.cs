using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Web;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using VoltProjects.Builder.Services;
using VoltProjects.Builder.Services.Storage;
using VoltProjects.Shared;
using VoltProjects.Shared.Collections;
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
    private readonly VoltProjectsBuilderConfig config;
    private readonly IStorageService storageService;
    private readonly Dictionary<string, Builder> builders;

    private static string[] supportedLangs = new[] { "csharp" };

    /// <summary>
    ///     Creates a new <see cref="BuildManager"/> instance
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="htmlMinifier"></param>
    /// <param name="highlightService"></param>
    /// <param name="config"></param>
    /// <param name="storageService"></param>
    /// <param name="serviceProvider"></param>
    public BuildManager(
        ILogger<BuildManager> logger,
        HtmlMinifier htmlMinifier,
        HtmlHighlightService highlightService,
        IOptions<VoltProjectsBuilderConfig> config,
        IStorageService storageService,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.htmlMinifier = htmlMinifier;
        this.highlightService = highlightService;
        this.config = config.Value;
        this.storageService = storageService;
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
    /// <param name="cancellationToken"></param>
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
        ProjectToc[] tocItems = await dbContext.UpsertProjectTOCs(buildResult.ProjectTocs, projectVersion);

        //Pre-Process pages
        ProjectPage[] pages = buildResult.ProjectPages;
        ListBuilder<string> projectImages = new();
        Parallel.ForEach(pages, (page, _) =>
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
                    projectImages.Add(imagePath);
                    
                    //TODO: Not do all of this shit
                    string imagePathInProject = Path.GetRelativePath(builtDocsLocation, imagePath);
                    string fileExtension = Path.GetExtension(imagePath);
                    string path = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag,
                        $"{imagePathInProject[..^fileExtension.Length]}.webp");

                    string newUrl = Path.Combine(config.StorageConfig.PublicUrl, path);
                    imageNode.SetAttributeValue("src", newUrl);
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
        
        //Process images
        List<StorageItem> imagesNeededToBeUploaded = new();
        IReadOnlyList<string> allProjectImages = projectImages.AsList();
        foreach (string projectImage in allProjectImages)
        {
            string imagePathInProject = Path.GetRelativePath(builtDocsLocation, projectImage);
            FileStream fileStream = File.OpenRead(projectImage);
            
            byte[] hash = await MD5.HashDataAsync(fileStream, cancellationToken);
            fileStream.Position = 0;
            string hashCombined = string.Join("", hash);
            
            ProjectStorageItem? foundResult = dbContext.ProjectStorageItems.AsNoTracking()
                .FirstOrDefault(x => x.ProjectVersionId == projectVersion.Id && x.Path == imagePathInProject);
            if (foundResult == null)
            {
                imagesNeededToBeUploaded.Add(await CreateImageWebp(fileStream, imagePathInProject, hashCombined, projectVersion, cancellationToken));
                continue;
            }
            
            //Compare hashes
            if (!string.Equals(hashCombined, foundResult.Hash, StringComparison.InvariantCultureIgnoreCase))
                imagesNeededToBeUploaded.Add(await CreateImageWebp(fileStream, imagePathInProject, hashCombined, projectVersion, cancellationToken));
        }
        
        //Now, to actually upload images
        await storageService.UploadBulkFileAsync(imagesNeededToBeUploaded.ToArray(), "image/webp", cancellationToken);

        //Add storage items to db for tracking
        if (imagesNeededToBeUploaded.Count > 0)
        {
            ProjectStorageItem[] upsertStorageItems = new ProjectStorageItem[imagesNeededToBeUploaded.Count];
            for (int i = 0; i < upsertStorageItems.Length; i++)
            {
                upsertStorageItems[i] = new ProjectStorageItem
                {
                    ProjectVersionId = projectVersion.Id,
                    Hash = imagesNeededToBeUploaded[i].Hash,
                    Path = imagesNeededToBeUploaded[i].OriginalFilePath
                };
            }

            await dbContext.UpsertProjectStorageAssets(upsertStorageItems);
        }
       
        await dbContext.UpsertProjectPages(pages, projectVersion);
    }

    private void ExecuteDocBuilderProcess(Builder builder, ProjectVersion projectVersion, string docsLocation, string docsBuiltLocation)
    {
        DocBuilder docBuilder = projectVersion.DocBuilder;

        string[]? arguments = docBuilder.Arguments;
        builder.PrepareBuilder(ref arguments, docsLocation, docsBuiltLocation);

        arguments ??= Array.Empty<string>();

        ProcessStartInfo processStartInfo = new(docBuilder.Application)
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
            throw new Exception("Builder failed to exit cleanly!");
        
        process.Dispose();
    }

    private async Task<StorageItem> CreateImageWebp(Stream imageStream, string imagePathProjectRel, string imageHash, ProjectVersion projectVersion, CancellationToken cancellationToken)
    {
        string fileExtension = Path.GetExtension(imagePathProjectRel);
        
        //We have the image file, convert to webp
        Image image = await Image.LoadAsync(imageStream, cancellationToken);

        MemoryStream imageMemoryStream = new();
        await image.SaveAsWebpAsync(imageMemoryStream, cancellationToken);
        image.Dispose();
                        
        imageMemoryStream.Position = 0;
        
        //Path in storage service that the file should live at
        string path = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag,
            $"{imagePathProjectRel[..^fileExtension.Length]}.webp");

        return new StorageItem
        {
            FileName = path,
            ItemStream = imageMemoryStream,
            Hash = imageHash,
            OriginalFilePath = imagePathProjectRel
        };
    }
}