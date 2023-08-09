using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main Projects View Controller
/// </summary>
public class ProjectController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(VoltProjectDbContext dbContext, ILogger<ProjectController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    /// <summary>
    ///     Main project view route endpoint
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="projectName"></param>
    /// <param name="version"></param>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    [HttpGet]
    [Route("/{projectName}/{version}/{**fullPath}")]
    public async Task<IActionResult> ProjectView(CancellationToken cancellationToken, string projectName, string version, string? fullPath)
    {
        //Try to get project first
        Project? project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == projectName, cancellationToken);
        if (project == null)
            return NotFound();
        
        //Try to get project version
        ProjectVersion? projectVersion = await dbContext.ProjectVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VersionTag == version && x.ProjectId == project.Id, cancellationToken);
        if (projectVersion == null)
            return GetProjectLatestRedirect(project, fullPath);

        //Set for ease of use in the view
        projectVersion.Project = project;

        //Default path (if none was provided)
        if (string.IsNullOrWhiteSpace(fullPath))
            fullPath = ".";
        
        //Now to find the page
        ProjectPage? projectPage =
            await dbContext.ProjectPages
                .AsNoTracking()
                .Include(x => x.ProjectToc)
                .FirstOrDefaultAsync(x => x.ProjectVersionId == projectVersion.Id && x.Path == fullPath, cancellationToken);

        //No page was found, all good then
        if (projectPage == null)
            return NotFound();

        //Set for ease of use in the view
        projectPage.ProjectVersion = projectVersion;
        
        //We have a page, lets do the rest of the work
        ProjectMenu? projectMenu = await dbContext.ProjectMenus
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectVersionId == projectVersion.Id, cancellationToken);
        if (projectMenu == null)
            throw new FileNotFoundException($"Project {project.Name} is missing it menu!");

        string requestPath = Request.Path;
        string baseProjectPath = $"/{Path.Combine(project.Name, projectVersion.VersionTag)}";
        
        //Menu stuff
        MenuItem[] menuItems = new MenuItem[projectMenu.LinkItem.Items!.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            LinkItem linkItem = projectMenu.LinkItem.Items[i];
            string href = Path.Combine(baseProjectPath, linkItem.Href!);
            menuItems[i] = new MenuItem
            {
                Title = linkItem.Title!,
                Href = href,
                IsActive = requestPath.Contains(href)
            };
        }
        
        //Figure out TOC stuff
        TocItem? tocItem = null;
        if (projectPage.ProjectToc != null)
            tocItem = ProcessTocItems(projectPage.ProjectToc.TocItem, projectPage.TocRel!, requestPath);

        return View("ProjectView", new ProjectViewModel
        {
            BasePath = baseProjectPath,
            ProjectPage = projectPage,
            MenuItems = menuItems,
            Toc = tocItem
        });
    }

    private IActionResult GetProjectLatestRedirect(Project project, string? fullPath)
    {
        //Find default version
        ProjectVersion? latestProjectVersion =
            dbContext.ProjectVersions.FirstOrDefault(x => x.IsDefault && x.ProjectId == project.Id);
            
        //All project must have a default, so this should never happen
        if (latestProjectVersion == null)
        {
            logger.LogError("Project {ProjectName} has no default project version!", project.Name);
            return NotFound();
        }
            
        return RedirectToAction("ProjectView", "Project", new {projectName = project.Name, version = latestProjectVersion.VersionTag, fullPath = fullPath});
    }

    private static TocItem ProcessTocItems(LinkItem linkItem, string pageRel, string requestPath)
    {
        bool isActive = linkItem.Href != null && requestPath.Contains(linkItem.Href);
        
        //Process child items first
        TocItem[]? children = linkItem.Items != null ? new TocItem[linkItem.Items.Length] : null;
        for (int i = 0; i < children?.Length; i++)
        {
            LinkItem childLinkItem = linkItem.Items![i];
            TocItem newChildItem = children[i] = ProcessTocItems(childLinkItem, pageRel, requestPath);
            
            //Child is active, then so will this one
            if (newChildItem.IsActive)
                isActive = true;
        }

        //Now root item
        TocItem newItem = new()
        {
            Title = linkItem.Title,
            Href = linkItem.Href != null ? Path.Combine(pageRel, linkItem.Href) : null,
            IsActive = isActive,
            Items = children
        };
        
        return newItem;
    }
}