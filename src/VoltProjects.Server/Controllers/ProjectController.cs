using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Telemetry;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main Projects View Controller
/// </summary>
public class ProjectController : Controller
{
    private readonly ProjectService projectService;
    private readonly VoltProjectsConfig config;

    public ProjectController(ProjectService projectService, IOptions<VoltProjectsConfig> config)
    {
        this.projectService = projectService;
        this.config = config.Value;
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
    [Route("/{projectName}/{version?}/{**fullPath}")]
    public async Task<IActionResult> ProjectView(string projectName, string? version, string? fullPath, CancellationToken cancellationToken)
    {
        using Activity projectActivity = Tracking.StartActivity(ActivityArea.Project, "main");
        
        //Default path (if none was provided)
        if (string.IsNullOrWhiteSpace(fullPath))
            fullPath = ".";
        
        //No version? Goto latest
        if (string.IsNullOrWhiteSpace(version))
            return await GetDefaultProjectRedirect(projectName, version, fullPath, cancellationToken);

        //Get project page
        ProjectPage? projectPage = await projectService.GetProjectPage(projectName, version, fullPath);

        //No page was found, try to get an external item
        if (projectPage == null)
        {
            //Try to get the project version first
            ProjectVersion? foundProjectVersion = await projectService.GetProjectVersion(projectName, version);

            //Invalid project version, redirect to latest
            if (foundProjectVersion == null)
                return await GetDefaultProjectRedirect(projectName, version, fullPath, cancellationToken);
            
            //Get project external storage item
            ProjectExternalItemStorageItem? externalItemStorageItem = await projectService.GetProjectExternalStorageItem(foundProjectVersion, fullPath);
            if (externalItemStorageItem == null)
                return NotFound();
            
            //Send user agent to storage item
            string storageItemUrl = $"{config.PublicUrl}{foundProjectVersion.Project.Name}/{foundProjectVersion.VersionTag}/{externalItemStorageItem.StorageItem.Path}";
            return RedirectPermanent(storageItemUrl);
        }

        //We have page, build out everything else
        ProjectVersion projectVersion = projectPage.ProjectVersion;
        Project project = projectVersion.Project;
        
        string requestPath = Request.Path;
        string baseProjectPath = $"/{Path.Combine(project.Name, projectVersion.VersionTag)}";

        //Build project nav
        ProjectNavModel navModel;
        using (Tracking.StartActivity(ActivityArea.Project, "nav"))
        {
            IReadOnlyList<MenuItem> menuItems = await projectService.GetProjectMenuItems(projectVersion, requestPath);
            navModel = new ProjectNavModel(projectVersion, menuItems);
        }
        
        //TOC Items
        IReadOnlyList<TocItem>? tocItems = await projectService.GetProjectPageTocItems(projectPage, requestPath);

        Uri baseUri = new(config.SiteUrl);
        Uri fullUrl = new(baseUri, requestPath);

        Response.Headers[HeaderNames.CacheControl] = $"public,max-age={config.CacheTime}";

        return View("ProjectView", new ProjectViewModel
        {
            BasePath = baseProjectPath,
            ProjectPage = projectPage,
            ProjectNavModel = navModel,
            ProjectHeaderModel = new ProjectHeaderModel
            {
                ProjectPage = projectPage,
                PageFullUrl = fullUrl
            },
            TocItems = tocItems
        });
    }

    private async Task<IActionResult> GetDefaultProjectRedirect(string projectName, string? previousVersion, string? fullPath, CancellationToken cancellationToken)
    {
        using Activity projectGetLatestActivity = Tracking.StartActivity(ActivityArea.Project, "getlatest");
        
        //Find default version
        ProjectVersion? defaultProject = await projectService.GetProjectDefaultVersion(projectName);
            
        //All project must have a default, so this should never happen
        if (defaultProject == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(fullPath) && !string.IsNullOrWhiteSpace(previousVersion))
            fullPath = Path.Combine(previousVersion, fullPath);
            
        return RedirectToAction("ProjectView", "Project", new { projectName, version = defaultProject.VersionTag, fullPath});
    }
}