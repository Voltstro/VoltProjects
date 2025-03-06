using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Telemetry;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main Projects View Controller
/// </summary>
public class ProjectController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly VoltProjectsConfig config;
    
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(
        VoltProjectDbContext dbContext,
        IOptions<VoltProjectsConfig> vpConfig,
        IMemoryCache memoryCache,
        ILogger<ProjectController> logger)
    {
        this.dbContext = dbContext;
        this.config = vpConfig.Value;
        this.memoryCache = memoryCache;
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
    [Route("/{projectName}/{version?}/{**fullPath}")]
    public async Task<IActionResult> ProjectView(string projectName, string? version, string? fullPath, CancellationToken cancellationToken)
    {
        using Activity projectActivity = Tracking.StartActivity(ActivityArea.Project, "main");
        
        //Default path (if none was provided)
        if (string.IsNullOrWhiteSpace(fullPath))
            fullPath = ".";
        
        //No version? Goto latest
        if (string.IsNullOrWhiteSpace(version))
            return await GetProjectLatestRedirect(projectName, version, fullPath, cancellationToken);

        /*
        ProjectPage? projectPage = await dbContext.ProjectPages
            .AsNoTracking()
            .Include(x => x.ProjectVersion)
                .ThenInclude(x => x.Project)
            .Include(x => 
                    x.ProjectVersion.MenuItems!.OrderBy(m => m.ItemOrder))
            .Include(x => x.Breadcrumbs.OrderBy(m => m.BreadcrumbOrder))
            .FirstOrDefaultAsync(x =>
                x.Path == fullPath &&
                x.ProjectVersion.VersionTag == version &&
                x.ProjectVersion.Project.Name == projectName &&
                x.Published,
                cancellationToken);
                */

        ProjectPage? projectPage = await ProjectPageQuery(dbContext, projectName, version, fullPath);

        //No page was found, try to get an external item
        if (projectPage == null)
        {
            //Try to get the project version first
            ProjectVersion? foundProjectVersion = await dbContext.ProjectVersions
                .AsNoTracking()
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.VersionTag == version && x.Project.Name == projectName,
                    cancellationToken: cancellationToken);

            //Invalid project version, redirect to latest
            if (foundProjectVersion == null)
                return await GetProjectLatestRedirect(projectName, version, fullPath, cancellationToken);
            
            ProjectExternalItemStorageItem? externalItemStorageItem =
                await ProjectExternalItemQuery(dbContext, foundProjectVersion.Id, fullPath);

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
            List<MenuItem> builtMenuItems = new();
            await foreach (ProjectMenuItem menuItem in ProjectMenuItemsQuery(dbContext, projectVersion.Id))
            {
                string menuPagePath = menuItem.Href;
                builtMenuItems.Add(new MenuItem
                {
                    Title = menuItem.Title,
                    Href = Path.Combine(baseProjectPath, menuPagePath),
                    IsActive = requestPath.Contains(menuPagePath)
                });
            }
            
            /*
            ProjectMenuItem[] menuItems = await ProjectMenuItemsQuery(dbContext, projectVersion.Id).ToArrayAsync(cancellationToken: cancellationToken);
            MenuItem[] builtMenuItems = new MenuItem[menuItems.Length];
            for (int i = 0; i < builtMenuItems.Length; i++)
            {
                string menuPagePath = menuItems[i].Href;
                builtMenuItems[i] = new MenuItem
                {
                    Title = menuItems[i].Title,
                    Href = Path.Combine(baseProjectPath, menuPagePath),
                    IsActive = requestPath.Contains(menuPagePath)
                };
            }
            */

            navModel = new ProjectNavModel
            {
                ProjectId = project.Id,
                ProjectName = project.DisplayName,
                BasePath = baseProjectPath,
                GitUrl = $"{project.GitUrl}/tree/{projectVersion.GitTag ?? projectVersion.GitBranch}",
                MenuItems = builtMenuItems
            };
        }
        
        //Figure out TOC stuff
        List<TocItem>? tocItems = null;
        if (projectPage.ProjectTocId != null)
        {
            tocItems = [];
            ProjectToc toc = await ProjectTocQuery(dbContext, projectPage.ProjectTocId.Value);

            foreach (ProjectTocItem projectTocItem in toc.TocItems)
            {
                TocItem builtTocItem = new()
                {
                    Id = projectTocItem.Id,
                    Title = projectTocItem.Title,
                    Href = projectTocItem.Href == null ? null : Path.Combine(projectPage.TocRel!, projectTocItem.Href),
                    IsActive = projectTocItem.Href != null && requestPath.Contains(projectTocItem.Href)
                };

                if (projectTocItem.ParentTocItemId != null)
                {
                    TocItem? parentTocItem = FindParentTocItem(tocItems, projectTocItem.ParentTocItemId.Value);
                    if (parentTocItem == null)
                    {
                        logger.LogWarning("Failed getting built parent TOC item");
                        break;
                    }

                    //Make parent active too
                    if (builtTocItem.IsActive)
                        parentTocItem.IsActive = true;

                    parentTocItem.Items ??= [];
                    parentTocItem.Items.Add(builtTocItem);
                }
                else
                {
                    tocItems.Add(builtTocItem);
                }
            }
        }

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

    private async Task<IActionResult> GetProjectLatestRedirect(string projectName, string? previousVersion, string? fullPath, CancellationToken cancellationToken)
    {
        using Activity projectGetLatestActivity = Tracking.StartActivity(ActivityArea.Project, "getlatest");
        
        //Find default version
        ProjectVersion? latestProjectVersion = await dbContext.ProjectVersions
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.IsDefault && x.Project.Name == projectName, cancellationToken);
            
        //All project must have a default, so this should never happen
        if (latestProjectVersion == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(fullPath) && !string.IsNullOrWhiteSpace(previousVersion))
            fullPath = Path.Combine(previousVersion, fullPath);
            
        return RedirectToAction("ProjectView", "Project", new { projectName, version = latestProjectVersion.VersionTag, fullPath});
    }

    private TocItem? FindParentTocItem(List<TocItem> tocItems, int childTocId)
    {
        foreach (TocItem item in tocItems)
        {
            if (item.Id == childTocId)
            {
                return item;
            }

            if (item.Items != null)
            {
                TocItem? result = FindParentTocItem(item.Items, childTocId);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    #region Queries
    
    //Main page query
    private static readonly Func<VoltProjectDbContext, string, string, string, Task<ProjectPage?>> ProjectPageQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext context, string projectName, string version, string path) => 
                context.ProjectPages
                    .AsNoTracking()
                    .Include(x => x.ProjectVersion)
                    .ThenInclude(x => x.Project)
                    .Include(x => x.Breadcrumbs.OrderBy(m => m.BreadcrumbOrder))
                    .FirstOrDefault(x =>
                        x.Path == path &&
                        x.ProjectVersion.VersionTag == version &&
                        x.ProjectVersion.Project.Name == projectName &&
                        x.Published));
    
    //External items query
    private static readonly Func<VoltProjectDbContext, int, string, Task<ProjectExternalItemStorageItem?>>
        ProjectExternalItemQuery =
            EF.CompileAsyncQuery(
                (VoltProjectDbContext context, int projectVersionId, string path) =>
                    context.ProjectExternalItemStorageItems
                        .AsNoTracking()
                        .Include(x => x.StorageItem)
                        .Include(x => x.ProjectExternalItem)
                        .ThenInclude(x => x.ProjectVersion)
                        .FirstOrDefault(x =>
                            x.StorageItem.Path == path &&
                            x.ProjectExternalItem.ProjectVersionId == projectVersionId));
    
    //Menu items query
    private static readonly Func<VoltProjectDbContext, int, IAsyncEnumerable<ProjectMenuItem>> ProjectMenuItemsQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext context, int projectVersionId) =>
                context.ProjectMenuItems
                    .AsNoTracking()
                    .OrderBy(x => x.ItemOrder)
                    .Where(x => x.ProjectVersionId == projectVersionId));

    //TOC query
    private static readonly Func<VoltProjectDbContext, int, Task<ProjectToc>> ProjectTocQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext context, int tocId) =>
                context.ProjectTocs
                    .AsNoTracking()
                    .Include(x => x.TocItems.OrderBy(y => y.ItemOrder))
                    .First(x => x.Id == tocId)
        );

    #endregion
}