using System.IO;
using System.Linq;
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
    ///     Re-direct project path
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    [Route("/{projectName}/{**fullPath}")]
    public IActionResult ProjectView(string projectName, string? fullPath)
    {
        //Try to get project first
        Project? project = dbContext.Projects.FirstOrDefault(x => x.Name == projectName);
        return project == null ? NotFound() : GetProjectLatestRedirect(project, fullPath);
    }

    [Route("/{projectName}/{version}/{**fullPath}")]
    public IActionResult ProjectView(string projectName, string version, string? fullPath)
    {
        //Try to get project first
        Project? project = dbContext.Projects.FirstOrDefault(x => x.Name == projectName);
        if (project == null)
            return NotFound();
        
        //Try to get project version
        ProjectVersion? projectVersion = dbContext.ProjectVersions.FirstOrDefault(x => x.VersionTag == version && x.ProjectId == project.Id);
        if (projectVersion == null)
            return GetProjectLatestRedirect(project, fullPath);

        //Default path (if none was provided)
        if (string.IsNullOrWhiteSpace(fullPath))
            fullPath = ".";
        
        //Now to find the page
        ProjectPage? projectPage =
            dbContext.ProjectPages.Include(x => x.ProjectToc).FirstOrDefault(x => x.ProjectVersionId == projectVersion.Id && x.Path == fullPath);

        //No page was found, all good then
        if (projectPage == null)
            return NotFound();
        
        //We have a page, lets do the rest of the work
        ProjectMenu? projectMenu = dbContext.ProjectMenus.FirstOrDefault(x => x.ProjectVersionId == projectVersion.Id);
        if (projectMenu == null)
            throw new FileNotFoundException($"Project {project.Name} is missing it menu!");
        
        //Figure out TOC stuff
        TocItem? tocItem = null;
        if (projectPage.ProjectToc != null)
            tocItem = ProcessTocItems(projectPage.ProjectToc.TocItem, projectPage.TocRel!, Request.Path);

        return View("ProjectView", new ProjectViewModel
        {
            ProjectPage = projectPage,
            ProjectMenu = projectMenu,
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
            
        return RedirectToAction("ProjectView", new {project = project, version = latestProjectVersion.VersionTag, catchAll = fullPath});
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