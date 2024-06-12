using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VoltProjects.Server.Models.Searching;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Controller for searching
/// </summary>
[Route("/search/")]
[ResponseCache(NoStore = true)]
public sealed class SearchController : Controller
{
    private readonly ILogger<SearchController> logger;
    private readonly IMemoryCache memoryCache;
    private readonly VoltProjectDbContext dbContext;
    private readonly SearchService searchService;
    
    public SearchController(
        ILogger<SearchController> logger,
        IMemoryCache memoryCache,
        VoltProjectDbContext dbContext,
        SearchService searchService)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.dbContext = dbContext;
        this.searchService = searchService;
    }
    
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Index(string query, int page, int size, int[] projectId, int[] projectVersionId, CancellationToken cancellationToken)
    {
        //Page should always be 1 or more
        page = page <= 0 ? 1 : page;

        //Size should either be 10, 25 or 50
        if (size != 5 && size != 10 && size != 25 && size != 50)
            size = 10;
        
        //Get all projects, this is cached, to save on DB calls and for better performance
        ProjectSearch[] projects = (await memoryCache.GetOrCreateAsync<ProjectSearch[]>("SearchProjects", async entry =>
        {
            Project[] projects = await dbContext.Projects
                .AsNoTracking()
                .Include(x => x.ProjectVersions)
                .ToArrayAsync(cancellationToken);

            ProjectSearch[] projectSelects = new ProjectSearch[projects.Length];
            for (int i = 0; i < projectSelects.Length; i++)
            {
                Project project = projects[i];

                //Create all project versions search
                ProjectVersionSearch[] projectVersions = new ProjectVersionSearch[project.ProjectVersions.Count];
                for (int j = 0; j < projectVersions.Length; j++)
                {
                    ProjectVersion version = project.ProjectVersions.GetItemByIndex(j);
                    projectVersions[j] = new ProjectVersionSearch(version.Id, version.VersionTag);
                }

                projectSelects[i] = new ProjectSearch(project.Id, project.DisplayName, projectVersions);
            }

            //Expiry in 6 hours
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return projectSelects;
        }))!;
        
        //Need to mark projects as active if query parameter includes them
        foreach (ProjectSearch project in projects)
        {
            project.Active = projectId.Contains(project.Id);

            foreach (ProjectVersionSearch projectVersion in project.Versions)
            {
                projectVersion.Active = projectVersionId.Contains(projectVersion.Id);
            }
        }
        
        //Do search if query contains something
        SearchPagedResult? pagedResult = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            //Get all active project IDs, or if none are selected, get all project IDs
            int[] activeProjectIds = projectId.Length >= 1
                ? projects.Where(x => x.Active).Select(x => x.Id).ToArray()
                : projects.Select(x => x.Id).ToArray();
            
            //Get all active project version IDs, or if none are selected, get all project version IDs
            int[] activeProjectVersionIds = projectVersionId.Length >= 1
                ? projects.Where(x => x.Active)
                    .SelectMany(x => x.Versions
                        .Where(y => y.Active)
                        .Select(z => z.Id))
                    .ToArray()
                : projects
                    .SelectMany(x => x.Versions
                        .Select(z => z.Id))
                    .ToArray();
            
            //Do actual search against DB
            pagedResult = await searchService.Search(dbContext, query, page, size, activeProjectIds, activeProjectVersionIds, cancellationToken);
        }

        //Depending on request method, we will change view
        if (Request.Method == "GET")
            return View(new SearchViewModel(projects, query, pagedResult));

        return View("SearchPreview", new SearchViewModel(projects, query, pagedResult));
    }
}