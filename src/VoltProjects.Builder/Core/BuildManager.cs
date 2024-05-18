using System.Diagnostics;
using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Builder.Core.Building.PageParsers;
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
    private readonly List<IPageParser> pageParsers;

    private static readonly string[] SupportedLangs = ["csharp"];

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

        pageParsers = new List<IPageParser>
        {
            new DescriptionParser(),
            new CodeParser(highlightService),
            new ImageParser(logger)
        };
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
        
        //Setup built docs location
        string builtDocsLocation = Path.Combine(projectPath, projectVersion.DocsBuiltPath);
        if(Directory.Exists(builtDocsLocation))
            Directory.Delete(builtDocsLocation, true);

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
        ListBuilder<IExternalObjectHandler> externalObjectsIncluded = new();
        Parallel.ForEach(pages, (page, _) =>
        {
            page.ProjectVersion = projectVersion;

            //Load page content into HtmlAgilityPack
            HtmlDocument doc = new();
            doc.LoadHtml(page.Content);

            //Run all page parses on this page
            foreach (IPageParser pageParser in pageParsers)
            {
                List<IExternalObjectHandler>? externalObjects = pageParser.FormatPage(builtDocsLocation, ref page, ref doc);
                if(externalObjects != null)
                    externalObjectsIncluded.AddRange(externalObjects);
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

            //Calculate hash
            page.PageHash = page.CalculateHash();
        });
        
        //Now handle project external items, adding onto our existing externalObjectsIncluded
        ProjectExternalItem[] externalItems = await dbContext.ProjectExternalItems
            .AsNoTracking()
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
            
            //Need to assign ProjectVersion, for GenericExternalObject constructor
            externalItem.ProjectVersion = projectVersion;

            string pathRelativePath = Path.GetRelativePath(builtDocsLocation, externalItemPath);
            GenericExternalObject externalObject = new(externalItemPath, pathRelativePath, externalItem);
            externalObjectsIncluded.Add(externalObject);
        }
        
        //Our final list
        IReadOnlyList<IExternalObjectHandler> finalStorageObjects = externalObjectsIncluded.AsList();
        logger.LogInformation("Have gotten {ExternalObjectsCount} number of external objects... checking which need to be uploaded...", finalStorageObjects.Count);
                
        //We need to find objects that need to be uploaded, either because they are new, or out-dated (hash check)
        List<IExternalObjectHandler> storageObjectsToUpload = new();

        //Pre-fetch all ProjectStorageItem first, as it will be quicker to sort through
        ProjectStorageItem[] projectStorageItems = await dbContext.ProjectStorageItems
            .AsNoTracking()
            .Where(x => x.ProjectVersionId == projectVersion.Id)
            .ToArrayAsync(cancellationToken);
        foreach (IExternalObjectHandler externalObject in finalStorageObjects)
        {
            //Check if this storage item exists
            ProjectStorageItem? foundResult =
                projectStorageItems.FirstOrDefault(x => x.Path == externalObject.PathInBuiltDocs);
            
            if (foundResult == null || !externalObject.Hash.Equals(foundResult.Hash, StringComparison.InvariantCultureIgnoreCase))
                storageObjectsToUpload.Add(externalObject);
        }
        
        //Now, to upload
        logger.LogInformation("Uploading {ExternalObjectUploadCount} number of objects to external object service...", storageObjectsToUpload.Count);
        await storageService.UploadBulkFileAsync(storageObjectsToUpload.ToArray(), cancellationToken);
        logger.LogInformation("Done uploading {ExternalObjectUploadCount} objects", storageObjectsToUpload.Count);
        
        //Upsert ProjectStorageItem to DB
        ProjectStorageItem[] storageItems = new ProjectStorageItem[storageObjectsToUpload.Count];
        for (int i = 0; i < storageObjectsToUpload.Count; i++)
        {
            IExternalObjectHandler item = storageObjectsToUpload[i];
            storageItems[i] = new ProjectStorageItem
            {
                ProjectVersionId = projectVersion.Id,
                Path = item.PathInBuiltDocs,
                Hash = item.Hash
            };
        }

        if(storageItems.Length > 0)
            storageItems = await dbContext.UpsertProjectStorageAssets(storageItems);
        
        //Upsert pages
        pages = await dbContext.UpsertProjectPages(pages, projectVersion);
        
        //Create and upsert ProjectExternalItemStorageItem
        IExternalObjectHandler[] uploadedExternalStorageItems = storageObjectsToUpload.Where(x => x.ExternalItem != null).ToArray();
        ProjectExternalItemStorageItem[] externalItemStorageItems =
            new ProjectExternalItemStorageItem[uploadedExternalStorageItems.Length];
        for (int i = 0; i < externalItemStorageItems.Length; i++)
        {
            IExternalObjectHandler externalObject = uploadedExternalStorageItems[i];
            ProjectStorageItem? upsertedStorageItem = storageItems.FirstOrDefault(x => x.Path == externalObject.PathInBuiltDocs);
            if (upsertedStorageItem == null)
            {
                logger.LogWarning("Failed to find DB id of storage item path {Path}", externalObject.PathInBuiltDocs);
                continue;
            }

            externalItemStorageItems[i] = new ProjectExternalItemStorageItem
            {
                ProjectExternalItemId = externalObject.ExternalItem!.Id,
                StorageItemId = upsertedStorageItem.Id
            };
        }

        //Upsert ProjectExternalItemStorageItems
        if (externalItemStorageItems.Length > 0)
            await dbContext.UpsertProjectExternalItemStorageItemItems(externalItemStorageItems);

        //TODO: Create ProjectPageStorageItems

        //Cleanup
        foreach (IExternalObjectHandler objectHandler in finalStorageObjects)
        {
            objectHandler.Dispose();
        }
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
}