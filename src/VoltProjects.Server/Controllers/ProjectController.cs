// ReSharper disable ExplicitCallerInfoArgument

using System;
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
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main Projects View Controller
/// </summary>
public class ProjectController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly ProjectMenuService projectMenuService;
    private readonly VoltProjectsConfig config;
    
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(
        VoltProjectDbContext dbContext,
        ProjectMenuService projectMenuService,
        IOptions<VoltProjectsConfig> vpConfig,
        IMemoryCache memoryCache,
        ILogger<ProjectController> logger)
    {
        this.dbContext = dbContext;
        this.projectMenuService = projectMenuService;
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
        using Activity? projectActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-Main");
        
        //Default path (if none was provided)
        if (string.IsNullOrWhiteSpace(fullPath))
            fullPath = ".";
        
        //No version? Goto latest
        if (string.IsNullOrWhiteSpace(version))
            return await GetProjectLatestRedirect(projectName, version, fullPath, cancellationToken);

        ProjectPage? projectPage = await dbContext.ProjectPages
            .AsNoTracking()
            .Include(x => x.ProjectVersion)
            .ThenInclude(x => x.Project)
            .FirstOrDefaultAsync(x =>
                x.Path == fullPath &&
                x.ProjectVersion.VersionTag == version &&
                x.ProjectVersion.Project.Name == projectName &&
                x.Published,
                cancellationToken);

        //No page was found, all good then
        if (projectPage == null)
        {
            //Try to get project version
            ProjectVersion? projectVersion = await dbContext.ProjectVersions
                .AsNoTracking()
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.VersionTag == version && x.Project.Name == projectName,
                    cancellationToken: cancellationToken);

            //Invalid project version
            if (projectVersion == null)
                return await GetProjectLatestRedirect(projectName, version, fullPath, cancellationToken);

            ProjectExternalItemStorageItem? externalItemStorageItem = dbContext.ProjectExternalItemStorageItems
                .AsNoTracking()
                .Include(x => x.StorageItem)
                .Include(x => x.ProjectExternalItem)
                .ThenInclude(x => x.ProjectVersion)
                .FirstOrDefault(x => x.StorageItem.Path == fullPath && x.ProjectExternalItem.ProjectVersionId == projectVersion.Id);

            if (externalItemStorageItem == null)
                return NotFound();
            
            //Send user agent to storage item
            string storageItemUrl = $"{config.PublicUrl}{projectVersion.Project.Name}/{projectVersion.VersionTag}/{externalItemStorageItem.StorageItem.Path}";
            return RedirectPermanent(storageItemUrl);
        }
        
        string requestPath = Request.Path;
        string baseProjectPath = $"/{Path.Combine(projectPage.ProjectVersion.Project.Name, projectPage.ProjectVersion.VersionTag)}";

        //Get project menu
        ProjectNavModel navModel;
        {
            Activity? projectNavActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-ProjectNav");
            navModel = await projectMenuService.GetProjectMenu(requestPath, baseProjectPath, projectPage.ProjectVersion, cancellationToken);
            projectNavActivity?.Dispose();
        }
        
        //Figure out TOC stuff
        TocItem? tocItem = null;
        if (projectPage.ProjectTocId != null)
            tocItem = await HandleProjectToc(projectPage, requestPath, cancellationToken);

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
            Toc = tocItem
        });
    }

    private async Task<IActionResult> GetProjectLatestRedirect(string projectName, string? previousVersion, string? fullPath, CancellationToken cancellationToken)
    {
        using Activity? projectGetLatestActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-GetLatest");
        
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

    private async Task<TocItem?> HandleProjectToc(ProjectPage projectPage, string requestPath, CancellationToken cancellationToken)
    {
        using Activity? projectHandleProjectTocActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-HandleProjectToc");
        
        int tocId = projectPage.ProjectTocId!.Value;
        string tocMemoryCacheKeyName = $"ProjectToc-{tocId}";
        ProjectToc? projectToc = await memoryCache.GetOrCreateAsync(tocMemoryCacheKeyName, async entry =>
        {
            using Activity? projectGetTocActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-GetProjectToc");
            ProjectToc? projectToc = await dbContext.ProjectTocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tocId, cancellationToken: cancellationToken);
                
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return projectToc;
        });

        TocItem? tocItem = null;
        if (projectToc != null)
        {
            Activity? projectProcessTocActivity = Tracking.TrackingActivitySource.StartActivity("ProjectView-ProcessToc");
            tocItem = ProcessTocItems(projectToc.TocItem, projectPage.TocRel!, requestPath);
            projectProcessTocActivity?.Dispose();
        }

        return tocItem;
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