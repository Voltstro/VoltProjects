using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using WebMarkupMin.Core;

namespace VoltProjects.Builder;

/// <summary>
///     Global build manager
/// </summary>
public sealed class BuildManager
{
    private readonly ILogger<BuildManager> logger;
    private readonly HtmlMinifier htmlMinifier;
    private readonly Dictionary<string, Builder> builders;
    
    public BuildManager(ILogger<BuildManager> logger, HtmlMinifier htmlMinifier, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.htmlMinifier = htmlMinifier;
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
    public void BuildProject(VoltProjectDbContext dbContext, ProjectVersion projectVersion)
    {
        //First, get the builder
        KeyValuePair<string, Builder>? buildFindResult = builders.FirstOrDefault(x => x.Key == projectVersion.DocBuilderId);
        if (buildFindResult == null)
            throw new NotImplementedException($"Builder {projectVersion.DocBuilderId} was not found!");

        //We have a builder
        Builder builder = buildFindResult.Value.Value;
        
        string projectPath = projectVersion.Project.GitUrl;
        bool isGitUrl = projectPath.StartsWith("https://");
        if (isGitUrl)
        {
            //TODO: Support git URLS
            throw new NotImplementedException();
        }

        //Check that project exists
        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException("Project directory not found!");
        
        //Check that docs exist
        string docsLocation = Path.Combine(projectPath, projectVersion.DocsPath);
        if (!Directory.Exists(docsLocation))
            throw new DirectoryNotFoundException("Failed to find docs dir!");
        
        //We will check this after the build
        string builtDocsLocation = Path.Combine(projectPath, projectVersion.DocsBuiltPath);
        
        //TODO: Run Pre-Build commands
        
        //First, run the build process
        builder.RunBuildProcess(docsLocation, builtDocsLocation);
        
        //Now check that built docs exist
        if (!Directory.Exists(builtDocsLocation))
            throw new DirectoryNotFoundException("Failed to find built docs dir!");

        //Build models
        BuildResult buildResult = builder.BuildProject(projectVersion, docsLocation, builtDocsLocation);
        
        //Upsert results into DB
        //Upsert project menu
        ProjectMenu projectMenu = buildResult.ProjectMenu;
        string json = JsonSerializer.Serialize(projectMenu.LinkItem);
        dbContext.Database
            .ExecuteSql(
                $"INSERT INTO public.\"ProjectMenu\" (\"ProjectVersionId\", \"LastUpdateTime\", \"LinkItem\") VALUES ({projectMenu.ProjectVersionId}, {projectMenu.LastUpdateTime}, {json}::jsonb) ON CONFLICT (\"ProjectVersionId\") DO UPDATE SET \"LastUpdateTime\" = EXCLUDED.\"LastUpdateTime\", \"LinkItem\" = EXCLUDED.\"LinkItem\" RETURNING *;");
        
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
        tocItems = dbContext.ProjectTocs.FromSqlRaw($"SELECT * FROM public.\"UpsertProjectTOCs\"(@p0, ARRAY[{tocParamsPlaceholder}]::upsertedtoc[]);", tocParams).ToArray();

        //Pre-Process pages
        ProjectPage[] pages = buildResult.ProjectPages;
        for (int i = 0; i < pages.Length; i++)
        {
            ProjectPage page = pages[i];

            //TODO: Syntax highlighting
            
            //Minify HTML
            page.Content = htmlMinifier.Minify(page.Content).MinifiedContent;
            
            //Handle TOC
            if (page.ProjectToc != null)
            {
                ProjectToc? toc = tocItems.FirstOrDefault(x => x.TocRel == page.ProjectToc.TocRel);
                page.ProjectTocId = toc.Id;
                page.ProjectToc = null;
            }

            pages[i] = page;
        }
        
        //Upsert pages
        int pageIndex = 1;
        int pageItemsCount = 10;
        object?[] pageParams = new object[1 + pages.Length * pageItemsCount];
        pageParams[0] = projectVersion.Id;
        string pageParamsPlaceholder = string.Join(",", pages.Select(x =>
        {
            pageParams[pageIndex] = x.PublishedDate;
            pageParams[pageIndex + 1] = x.Title;
            pageParams[pageIndex + 2] = x.TitleDisplay;
            pageParams[pageIndex + 3] = x.Aside;
            pageParams[pageIndex + 4] = x.WordCount;
            pageParams[pageIndex + 5] = x.ProjectTocId;
            pageParams[pageIndex + 6] = x.TocRel;
            pageParams[pageIndex + 7] = x.Path;
            pageParams[pageIndex + 8] = x.Description;
            pageParams[pageIndex + 9] = x.Content;

            return $"ROW(@p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++})";
        }));

        //Upsert project pages
        //No return needed on this one, so we will use dbContext.Database
        dbContext.Database.ExecuteSqlRaw(
            $"SELECT public.\"UpsertProjectPages\"(@p0, ARRAY[{pageParamsPlaceholder}]::upsertedpage[]);", pageParams);
    }
}