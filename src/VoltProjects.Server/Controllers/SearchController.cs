using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Server.Models.Searching;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Telemetry;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Controller for searching
/// </summary>
[Route("/search/")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class SearchController : Controller
{
    private readonly SearchService searchService;
    private readonly VoltProjectDbContext dbContext;
    
    public SearchController(SearchService searchService, VoltProjectDbContext dbContext)
    {
        this.searchService = searchService;
        this.dbContext = dbContext;
    }
    
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Index(string? query, int page, int? size, string? project, CancellationToken cancellationToken)
    {
        IEnumerable<KeyValuePair<string, object?>> tags = new List<KeyValuePair<string, object?>>
        {
            new("requestMethod", Request.Method),
            new("query", query),
            new("page", page),
            new("size", size),
            new("project", project)
        };
        
        using Activity searchActivity = Tracking.StartActivity(ActivityArea.Search, "main", tags: tags);
        
        //Fetch projects and versions
        ProjectVersion[] projects = dbContext.ProjectVersions
            .AsNoTracking()
            .Include(x => x.Project)
            .OrderBy(x => x.Project.Name)
            .ThenBy(x => x.VersionTag)
            .ToArray();
        
        //Figure out what project is active
        ProjectVersion? selectedProject = null;
        string[]? projectValues = project?.Split("|");
        if (projectValues?.Length == 2)
        {
            string projectIdStr = projectValues[0];
            string projectVersionIdStr = projectValues[1];
            
            //Both need to be ints
            if(!int.TryParse(projectIdStr, out int projectId) || !int.TryParse(projectVersionIdStr, out int projectVersionId))
                return BadRequest();
            
            selectedProject = projects.FirstOrDefault(x => x.Id == projectVersionId && x.ProjectId == projectId);
        }

        //Do search query (if we have project and query)
        PagedResult<SearchResult>? pages = null;
        if (!string.IsNullOrEmpty(query) && selectedProject != null)
        {
            size ??= 15;
            pages = await searchService.Search(query, selectedProject.ProjectId, selectedProject.Id, page, size.Value, cancellationToken);
        }

        SearchViewModel searchViewModel = new(query, projects, selectedProject, pages);
        return Request.Method == "GET" ? View(searchViewModel) : View("SearchPreview", searchViewModel);
    }
}