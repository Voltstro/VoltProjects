using System.Diagnostics;
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
        ListBuilder<BuildProjectImage> projectImages = new();
        Parallel.ForEach(pages, (page, _) =>
        {
            page.ProjectVersion = projectVersion;
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
            HtmlNodeCollection? images = doc.DocumentNode.SelectNodes("//img/@src");
            if (images != null)
            {
                foreach (HtmlNode imageNode in images)
                {
                    HtmlAttribute srcAttribute = imageNode.Attributes["src"];
                    
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
                    string fullImagePath = Path.Combine(builtDocsLocation, imagePathInProject);
                    BuildProjectImage image = new BuildProjectImage(config, page, imagePathInProject, fullImagePath);
                    projectImages.Add(image);
                    
                    imageNode.SetAttributeValue("src", image.FullImagePath);
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

            page.PageHash = page.CalculateHash();
        });
        
        //Process and upload images
        List<StorageItem> storageItemsToUpload = [];
        foreach (BuildProjectImage projectImage in projectImages.AsList())
        {
            string hash = projectImage.Hash;
            
            //Do we have this image already?
            ProjectStorageItem? foundResult = dbContext.ProjectStorageItems.AsNoTracking()
                .FirstOrDefault(x => x.ProjectVersionId == projectVersion.Id && x.Path == projectImage.FullImagePath);
            
            //No? Upload it
            if (foundResult == null || !hash.Equals(foundResult.Hash, StringComparison.InvariantCultureIgnoreCase))
            {
                StorageItem image = await CreateImageWebp(projectImage, projectImage.FileStream, projectImage.Hash,
                    cancellationToken);
                storageItemsToUpload.Add(image);
                projectImage.StorageItem = image;
            }
        }
        
        //External items
        ProjectExternalItem[] externalItems =
            await dbContext.ProjectExternalItems
                .Where(x => x.ProjectVersionId == projectVersion.Id)
                .ToArrayAsync(cancellationToken);

        foreach (ProjectExternalItem externalItem in externalItems)
        {
            string externalItemPath = Path.Combine(builtDocsLocation, externalItem.Path);
            if (!File.Exists(externalItemPath))
            {
                logger.LogWarning("Could not find external item {ExternalItem}!", externalItemPath);
                continue;
            }

            FileStream fileStream = File.OpenRead(externalItemPath);
            string hash = Helper.GetFileHash(fileStream);
            
            //See if this file has been uploaded already
            ProjectExternalItemStorageItem? externalItemStorageItem =
                await dbContext.ProjectExternalItemStorageItems
                    .Include(x => x.StorageItem)
                    .FirstOrDefaultAsync(x => x.StorageItemId == externalItem.Id, cancellationToken);

            if (externalItemStorageItem == null || !hash.Equals(externalItemStorageItem.StorageItem.Hash, StringComparison.InvariantCultureIgnoreCase))
            {
                //Upload it
                StorageItem storageItem = new()
                {
                    FileName = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag, externalItem.Path),
                    ItemStream = fileStream,
                    Hash = hash,
                    OriginalFilePath = externalItem.Path,
                    ContentType = "text/yaml"
                };
                storageItemsToUpload.Add(storageItem);
            }
        }
        
        //Upload storage items
        await storageService.UploadBulkFileAsync(storageItemsToUpload.ToArray(), cancellationToken);

        //Close streams and convert StorageItems into ProjectStorageItems
        ProjectStorageItem[] storageItems = new ProjectStorageItem[storageItemsToUpload.Count];
        for (int i = 0; i < storageItemsToUpload.Count; i++)
        {
            StorageItem item = storageItemsToUpload[i];
            await item.ItemStream.DisposeAsync();
            storageItems[i] = new ProjectStorageItem
            {
                ProjectVersionId = projectVersion.Id,
                Path = item.OriginalFilePath,
                Hash = item.Hash
            };
        }

        //Upsert storage assets
        storageItems = await dbContext.UpsertProjectStorageAssets(storageItems);
        
        //Upsert pages
        pages = await dbContext.UpsertProjectPages(pages, projectVersion);

        //Create page storage items
        List<ProjectPageStorageItem> pageStorageItems = [];
        pageStorageItems.AddRange(projectImages.AsList().Where(x => x.StorageItem != null)
            .Select(projectImage => new ProjectPageStorageItem()
            {
                StorageItemId = storageItems.FirstOrDefault(x => x.Path == projectImage.OriginalImagePathInProject)!.Id,
                PageId = pages.FirstOrDefault(x => x.Path == projectImage.ProjectPage.Path)!.Id
            }));

        await dbContext.UpsertProjectPageStorageItems(pageStorageItems.ToArray());
        
        //Create external item storage items
        List<ProjectExternalItemStorageItem> externalItemStorageItems = [];
        externalItemStorageItems
            .AddRange(externalItems.Select(externalItem => new ProjectExternalItemStorageItem { ProjectExternalItemId = externalItem.Id, StorageItemId = storageItems.FirstOrDefault(x => x.Path == externalItem.Path)!.Id }));

        await dbContext.UpsertProjectExternalItemStorageItemItems(externalItemStorageItems.ToArray());
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

    private async Task<StorageItem> CreateImageWebp(BuildProjectImage buildProjectImage, Stream imageStream, string hash, CancellationToken cancellationToken)
    {
        if (Path.GetExtension(buildProjectImage.FullImagePath) != ".webp")
        {
            //Convert image to webp
            Image image = await Image.LoadAsync(imageStream, cancellationToken);
            
            //Close the arial file stream
            await imageStream.DisposeAsync();
            
            //Convert to webp
            imageStream = new MemoryStream();
            await image.SaveAsWebpAsync(imageStream, cancellationToken);
            image.Dispose();

            imageStream.Position = 0;
        }

        return new StorageItem
        {
            FileName = buildProjectImage.ImagePath,
            ItemStream = imageStream,
            Hash = hash,
            OriginalFilePath = buildProjectImage.OriginalImagePathInProject,
            ContentType = "image/webp"
        };
    }
}