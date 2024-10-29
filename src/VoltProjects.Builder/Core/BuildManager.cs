using System.Diagnostics;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Builder.Core.Building.PageParsers;
using VoltProjects.Builder.Builders;
using VoltProjects.Builder.Services;
using VoltProjects.Builder.Services.Storage;
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
    private readonly IStorageService storageService;
    private readonly Dictionary<string, IBuilder> builders;
    private readonly List<IPageParser> pageParsers;

    /// <summary>
    ///     Creates a new <see cref="BuildManager"/> instance
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="htmlMinifier"></param>
    /// <param name="highlightService"></param>
    /// <param name="storageService"></param>
    /// <param name="serviceProvider"></param>
    public BuildManager(
        ILogger<BuildManager> logger,
        HtmlMinifier htmlMinifier,
        HtmlHighlightService highlightService,
        IStorageService storageService,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.htmlMinifier = htmlMinifier;
        this.storageService = storageService;
        builders = new Dictionary<string, IBuilder>();

        IEnumerable<Type> foundBuilders = ReflectionHelper.GetAssignedFrom<IBuilder>();

        //Create all builders
        foreach (Type foundBuilder in foundBuilders)
        {
            BuilderNameAttribute attribute = (BuilderNameAttribute)Attribute.GetCustomAttribute(foundBuilder, typeof(BuilderNameAttribute))!;

            IBuilder builder = (IBuilder)ActivatorUtilities.CreateInstance(serviceProvider, foundBuilder);
            builders.Add(attribute.Name, builder);
            this.logger.LogDebug("Created builder {Builder}", attribute.Name);

            Attribute? obsoleteAttribute = Attribute.GetCustomAttribute(foundBuilder, typeof(ObsoleteAttribute));
            if (obsoleteAttribute != null)
                this.logger.LogWarning("Notice: Builder {BuilderName} has been marked as obsolete. {ObsoleteMessage}", attribute.Name, (obsoleteAttribute as ObsoleteAttribute)!.Message);
        }

        pageParsers = new List<IPageParser>
        {
            new DescriptionParser(),
            new CodeParser(highlightService),
            new SrcAttributeParser(logger, storageService)
        };
    }

    /// <summary>
    ///     Builds a select project
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectVersion"></param>
    /// <param name="projectPath"></param>
    /// <param name="cancellationToken"></param>
    public async Task<BuildProjectResult> BuildProject(VoltProjectDbContext dbContext, ProjectVersion projectVersion, string projectPath, CancellationToken cancellationToken)
    {
        //First, get the builder
        KeyValuePair<string, IBuilder>? buildFindResult = builders.FirstOrDefault(x => x.Key == projectVersion.DocBuilderId);
        if (buildFindResult == null)
            throw new NotImplementedException($"Builder {projectVersion.DocBuilderId} was not found!");

        //We have a builder
        IBuilder builder = buildFindResult.Value.Value;

        //Check that docs exist
        string docsLocation = Path.Combine(projectPath, projectVersion.DocsPath);
        if (!Directory.Exists(docsLocation))
            throw new DirectoryNotFoundException("Failed to find docs dir!");
        
        //Setup built docs location
        string builtDocsLocation = Path.Combine(projectPath, projectVersion.DocsBuiltPath);
        if(Directory.Exists(builtDocsLocation))
            Directory.Delete(builtDocsLocation, true);

        List<ProjectBuildEventLog> buildEventLogs = new();
        
        ProjectPreBuild[] prebuildCommands = await dbContext.PreBuildCommands
            .AsNoTracking()
            .Where(x => x.ProjectVersionId == projectVersion.Id)
            .OrderBy(x => x.Order)
            .ToArrayAsync(cancellationToken);
        
        //Run pre-build, then build
        buildEventLogs.AddRange(ExecuteDocPreBuildCommands(prebuildCommands, projectPath));
        buildEventLogs.AddRange(ExecuteDocBuilderProcess(builder, projectVersion, docsLocation, builtDocsLocation));
        
        //Now check that built docs exist
        if (!Directory.Exists(builtDocsLocation))
            throw new DirectoryNotFoundException("Failed to find built docs dir!");

        //Build models
        BuildResult buildResult = builder.BuildProject(projectVersion, docsLocation, builtDocsLocation);
        
        //Upsert project menu items, and delete any unused ones
        ProjectMenuItem[] menuItems = await dbContext.UpsertProjectMenuItems(buildResult.ProjectMenuItems);
        await dbContext.ProjectMenuItems
            .Where(p => !menuItems.Contains(p) && p.ProjectVersionId == projectVersion.Id)
            .ExecuteDeleteAsync(cancellationToken);
        
        //Upsert project TOCs
        ProjectToc[] tocItems = await dbContext.UpsertProjectTocs(buildResult.ProjectTocs);
        
        //Deal with project TOC items
        List<ProjectTocItem> rootTocItems = buildResult.ProjectTocItems.Where(x => x.ParentTocItem == null).ToList();
        foreach (ProjectTocItem rootTocItem in rootTocItems)
        {
            ProjectToc tocItem = tocItems.First(x => x.TocRel == rootTocItem.ProjectToc.TocRel);
            rootTocItem.ProjectTocId = tocItem.Id;
        }
        
        rootTocItems = (await dbContext.UpsertProjectTocItems(rootTocItems.ToArray())).ToList();
        
        //Now child TOC items
        List<ProjectTocItem> childTocItems = buildResult.ProjectTocItems.Where(x => x.ParentTocItem != null).ToList();
        List<ProjectTocItem> upsertTocItems = [];
        while (childTocItems.Count > 0)
        {
            int lastChildTocItemSize = childTocItems.Count;
            upsertTocItems.Clear();
            for (int i = 0; i < childTocItems.Count; i++)
            {
                ProjectTocItem childTocItem = childTocItems[i];
                ProjectTocItem? rootTocItem = rootTocItems.FirstOrDefault(x => x.Href == childTocItem.ParentTocItem!.Href && x.Title == childTocItem.ParentTocItem.Title);
                if(rootTocItem == null)
                    continue;

                childTocItem.ProjectTocId = tocItems.First(x => x.TocRel == childTocItem.ProjectToc.TocRel).Id;
                childTocItem.ParentTocItemId = rootTocItem.Id;
                upsertTocItems.Add(childTocItem);
                childTocItems.Remove(childTocItem);
            }

            //Stop infinite loops
            if (lastChildTocItemSize == childTocItems.Count)
                throw new Exception(
                    $"{lastChildTocItemSize} TOC items existed before child's parents were processed, but by the end there was still that many!");
            
            //Upsert this batch
            ProjectTocItem[] batchUpsertResults = await dbContext.UpsertProjectTocItems(upsertTocItems.ToArray());
            rootTocItems.AddRange(batchUpsertResults);
        }

        // Delete any not upsertted
        await dbContext.ProjectTocItems
            .Where(p => !rootTocItems.Contains(p) && p.ProjectVersionId == projectVersion.Id)
            .ExecuteDeleteAsync(cancellationToken);

        //Pre-Process pages
        ProjectPage[] pages = buildResult.ProjectPages;
        List<IExternalObjectHandler> externalObjectsIncluded = new();
        foreach (ProjectPage page in pages)
        {
            page.ProjectVersionId = projectVersion.Id;
            page.ProjectVersion = projectVersion;
            page.LanguageConfiguration = projectVersion.Language.Configuration;

            //Load page content into HtmlAgilityPack
            HtmlDocument doc = new();
            doc.LoadHtml(page.Content);

            //Run all page parses on this page
            foreach (IPageParser pageParser in pageParsers)
            {
                pageParser.FormatPage(builtDocsLocation, page, ref externalObjectsIncluded, ref doc);
            }
            
            //New HTML Content
            string content = doc.DocumentNode.OuterHtml;

            //Minify HTML
            page.Content = htmlMinifier.Minify(content).MinifiedContent;

            //Handle TOC
            if (page.ProjectToc != null)
            {
                ProjectToc? toc = tocItems.FirstOrDefault(x => x.TocRel == page.ProjectToc.TocRel);
                if (toc == null)
                    logger.LogWarning("Page {PagePath} has a TOC rel of {TocRel}, which was not found in the upserted TOCs!", page.Path, page.ProjectToc.TocRel);
                else
                    page.ProjectTocId = toc.Id;
            }

            //Calculate hash
            page.PageHash = page.CalculateHash();
        }
        
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
        logger.LogInformation("Have gotten {ExternalObjectsCount} number of external objects... checking which need to be uploaded...", externalObjectsIncluded.Count);
                
        //We need to find objects that need to be uploaded, either because they are new, or out-dated (hash check)
        List<IExternalObjectHandler> storageObjectsToUpload = new();

        //Pre-fetch all ProjectStorageItem first, as it will be quicker to sort through
        ProjectStorageItem[] projectStorageItems = await dbContext.ProjectStorageItems
            .AsNoTracking()
            .Where(x => x.ProjectVersionId == projectVersion.Id)
            .ToArrayAsync(cancellationToken);
        foreach (IExternalObjectHandler externalObject in externalObjectsIncluded)
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
        pages = await dbContext.UpsertProjectPages(pages);
        await dbContext.ProjectPages
            .Where(p => !pages.Contains(p) && p.ProjectVersionId == projectVersion.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(b => b.Published, false), cancellationToken);

        //Creation and upserting of ProjectExternalItemStorageItem
        {
            //Create ProjectExternalItemStorageItems
            IExternalObjectHandler[] uploadedExternalStorageItems = storageObjectsToUpload.Where(x => x.ExternalItem != null).ToArray();
            ProjectExternalItemStorageItem[] externalItemStorageItems =
                new ProjectExternalItemStorageItem[uploadedExternalStorageItems.Length];
            for (int i = 0; i < uploadedExternalStorageItems.Length; i++)
            {
                IExternalObjectHandler externalObject = uploadedExternalStorageItems[i];
            
                ProjectStorageItem upsertedStorageItem = storageItems.First(x => x.Path == externalObject.PathInBuiltDocs);

                externalItemStorageItems[i] = new ProjectExternalItemStorageItem
                {
                    ProjectExternalItemId = externalObject.ExternalItem!.Id,
                    StorageItemId = upsertedStorageItem.Id
                };
            }

            //Upsert ProjectExternalItemStorageItems
            if (externalItemStorageItems.Length > 0)
                await dbContext.UpsertProjectExternalItemStorageItemItems(externalItemStorageItems);
        }

        //Creation and upserting ProjectPageStorageItem
        {
            //Create ProjectPageStorageItems
            IExternalObjectHandler[] uploadedPageStorageItems =
                storageObjectsToUpload.Where(x => x.ProjectPages.Count > 0).ToArray();
            List<ProjectPageStorageItem> projectPageStorageItems = new();
            foreach (IExternalObjectHandler externalObject in uploadedPageStorageItems)
            {
                foreach (ProjectPage projectPage in externalObject.ProjectPages)
                {
                    ProjectPage upsertedPage = pages.First(x => x.Path == projectPage.Path);
                    IExternalObjectHandler o = externalObject;
                    ProjectStorageItem upsertedStorageItem = storageItems.First(x => x.Path == o.PathInBuiltDocs);
                    
                    projectPageStorageItems.Add(new ProjectPageStorageItem
                    {
                        PageId = upsertedPage.Id,
                        StorageItemId = upsertedStorageItem.Id
                    });
                }
            }
        
            //Upsert ProjectPageStorageItems
            if (projectPageStorageItems.Count > 0)
                await dbContext.UpsertProjectPageStorageItems(projectPageStorageItems.ToArray());
        }

        //Cleanup
        foreach (IExternalObjectHandler objectHandler in externalObjectsIncluded)
        {
            objectHandler.Dispose();
        }

        return new BuildProjectResult
        {
            BuildEventLogs = buildEventLogs
        };
    }

    private List<ProjectBuildEventLog> ExecuteDocPreBuildCommands(ProjectPreBuild[] preBuildCommands, string repoPath)
    {
        //Run prebuild
        List<ProjectBuildEventLog> eventLogs = new();
        foreach (ProjectPreBuild prebuildCommand in preBuildCommands)
        {
            string? arguments = prebuildCommand.Arguments;
            arguments ??= string.Empty;
            ProcessStartInfo startInfo = new(prebuildCommand.Command, arguments)
            {
                WorkingDirectory = repoPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            
            Process process = new()
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, args) =>
            {
                string? data = args.Data;
                if(string.IsNullOrWhiteSpace(data))
                    return;
                
                logger.LogInformation("[Pre-Build Process]: {Data}", data);
                eventLogs.Add(new ProjectBuildEventLog
                {
                    LogLevelId = 1,
                    Date = DateTime.UtcNow,
                    Message = data
                });
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                string? data = args.Data;
                if(string.IsNullOrWhiteSpace(data))
                    return;
                
                logger.LogError("[Pre-Build Process]: {Data}", data);
                eventLogs.Add(new ProjectBuildEventLog
                {
                    LogLevelId = 2,
                    Date = DateTime.UtcNow,
                    Message = data
                });
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelErrorRead();
            process.CancelOutputRead();

            if (process.ExitCode != 0)
                throw new Exception("Action process failed to run!");
            
            process.Dispose();
        }

        return eventLogs;
    }

    private List<ProjectBuildEventLog> ExecuteDocBuilderProcess(IBuilder builder, ProjectVersion projectVersion, string docsLocation, string docsBuiltLocation)
    {
        DocBuilder docBuilder = projectVersion.DocBuilder;

        string[]? arguments = docBuilder.Arguments;
        builder.PrepareBuilder(ref arguments, docsLocation, docsBuiltLocation);

        arguments ??= [];

        ProcessStartInfo processStartInfo = new(docBuilder.Application, string.Join(' ', arguments))
        {
            WorkingDirectory = docsLocation,
            RedirectStandardError = true,
            RedirectStandardOutput = true
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

        List<ProjectBuildEventLog> eventLogs = new List<ProjectBuildEventLog>();
        Process process = new();
        process.StartInfo = processStartInfo;
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += (sender, args) =>
        {
            string? data = args.Data;
            if(string.IsNullOrWhiteSpace(data))
                return;
            logger.LogInformation("[Build Process]: {Data}", data);
            eventLogs.Add(new ProjectBuildEventLog
            {
                LogLevelId = 1,
                Date = DateTime.UtcNow,
                Message = data
            });
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            string? data = args.Data;
            if(string.IsNullOrWhiteSpace(data))
                return;
            logger.LogError("[Build Process]: {Data}", data);
            eventLogs.Add(new ProjectBuildEventLog
            {
                LogLevelId = 2,
                Date = DateTime.UtcNow,
                Message = data
            });
        };
        process.Start();

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        
        process.CancelErrorRead();
        process.CancelOutputRead();

        if (process.ExitCode != 0)
            throw new Exception("Builder failed to exit cleanly!");
        
        process.Dispose();
        return eventLogs;
    }
}