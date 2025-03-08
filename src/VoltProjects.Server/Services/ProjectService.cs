using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Server.Models;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Services;

/// <summary>
///     Backing service for anything related to <see cref="Project"/>, <see cref="ProjectVersion"/> or <see cref="ProjectPage"/>
/// </summary>
public sealed class ProjectService
{
    private readonly VoltProjectDbContext dbContext;
    
    public ProjectService(VoltProjectDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    /// <summary>
    ///     Gets a <see cref="ProjectPage"/>. Can return null if either the page doesn't exist, or project/project version doesn't exist
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="projectVersion"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<ProjectPage?> GetProjectPage(string projectName, string projectVersion, string path)
    {
        ProjectPage? projectPage = await ProjectPageQuery(dbContext, projectName, projectVersion, path);
        return projectPage;
    }

    /// <summary>
    ///     Gets a <see cref="ProjectVersion"/>'s menu items
    /// </summary>
    /// <param name="projectVersion"></param>
    /// <param name="requestPath"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<MenuItem>> GetProjectMenuItems(ProjectVersion projectVersion, string? requestPath)
    {
        string baseProjectPath = $"/{Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag)}";
        List<MenuItem> builtMenuItems = [];
        await foreach (ProjectMenuItem menuItem in ProjectMenuItemsQuery(dbContext, projectVersion.Id))
        {
            string menuPagePath = menuItem.Href;
            builtMenuItems.Add(new MenuItem
            {
                Title = menuItem.Title,
                Href = Path.Combine(baseProjectPath, menuPagePath),
                IsActive = requestPath?.Contains(menuPagePath) ?? false
            });
        }

        return builtMenuItems;
    }

    /// <summary>
    ///     Gets a <see cref="ProjectPage"/> TOC items. Returns null if page doesn't have a TOC
    /// </summary>
    /// <param name="projectPage"></param>
    /// <param name="requestPath"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<TocItem>?> GetProjectPageTocItems(ProjectPage projectPage, string requestPath)
    {
        if(projectPage.ProjectTocId == null)
            return null;
        
        List<TocItem> tocItems = [];
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
                    //logger.LogWarning("Failed getting built parent TOC item");
                    break;
                }

                //Make parent active too
                if (builtTocItem.IsActive)
                    parentTocItem.IsActive = true;

                parentTocItem.Items ??= [];
                parentTocItem.Items.Add(builtTocItem);
            }
            else
                tocItems.Add(builtTocItem);
        }
        
        return tocItems;
    }

    /// <summary>
    ///     Gets a <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="versionTag"></param>
    /// <returns></returns>
    public async Task<ProjectVersion?> GetProjectVersion(string projectName, string versionTag)
    {
        ProjectVersion? projectVersion = await GetProjectVersionQuery(dbContext, projectName, versionTag);
        return projectVersion;
    }
    
    /// <summary>
    ///     Gets the default <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public async Task<ProjectVersion?> GetProjectDefaultVersion(string projectName)
    {
        ProjectVersion? latestProjectVersion = await GetProjectDefaultVersionQuery(dbContext, projectName);
        return latestProjectVersion;
    }

    /// <summary>
    ///     Gets a <see cref="ProjectVersion"/> storage item
    /// </summary>
    /// <param name="projectVersion"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<ProjectExternalItemStorageItem?> GetProjectExternalStorageItem(ProjectVersion projectVersion, string path)
    {
        ProjectExternalItemStorageItem? storageItem = await ProjectExternalItemQuery(dbContext, projectVersion.Id, path);
        return storageItem;
    }
    
    private static TocItem? FindParentTocItem(List<TocItem> tocItems, int childTocId)
    {
        foreach (TocItem item in tocItems)
        {
            if (item.Id == childTocId)
                return item;

            if (item.Items == null)
                continue;
            
            TocItem? result = FindParentTocItem(item.Items, childTocId);
            if (result != null)
                return result;
        }

        return null;
    }
    
    //Project default version query
    private static readonly Func<VoltProjectDbContext, string, Task<ProjectVersion?>> GetProjectDefaultVersionQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext dbContext, string projectName) =>
                dbContext.ProjectVersions
                    .AsNoTracking()
                    .Include(x => x.Project)
                    .FirstOrDefault(x => x.IsDefault && x.Project.Name == projectName));
    
    //Project version query
    private static readonly Func<VoltProjectDbContext, string, string, Task<ProjectVersion?>> GetProjectVersionQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext dbContext, string projectName, string versionTag) =>
                dbContext.ProjectVersions
                    .AsNoTracking()
                    .Include(x => x.Project)
                    .FirstOrDefault(x => x.VersionTag == versionTag && x.Project.Name == projectName));
    
    //Page query
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
}