using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Services;

/// <summary>
///     Service for getting a project's menu
/// </summary>
public sealed class ProjectMenuService
{
    private readonly VoltProjectDbContext dbContext;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    ///     Creates a new <see cref="ProjectMenuService"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="memoryCache"></param>
    public ProjectMenuService(VoltProjectDbContext dbContext, IMemoryCache memoryCache)
    {
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    ///     Gets a project's menu
    /// </summary>
    /// <param name="requestPath"></param>
    /// <param name="baseProjectPath"></param>
    /// <param name="project"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<ProjectNavModel> GetProjectMenu(string requestPath, string baseProjectPath, ProjectVersion project, CancellationToken cancellationToken)
    {
        string memoryCacheKeyName = $"ProjectMenu-{project.Id}";
        ProjectMenu? projectMenu = await memoryCache.GetOrCreateAsync(memoryCacheKeyName, async entry =>
        {
            ProjectMenu? projectMenu = await dbContext.ProjectMenus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectVersionId == project.Id, cancellationToken);

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return projectMenu;
        });
        
        if (projectMenu == null)
            throw new FileNotFoundException($"Project {project.Project.Name} is missing it menu!");
        
        //Process each menu item
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

        return new ProjectNavModel
        {
            ProjectId = project.ProjectId,
            ProjectVersionId = project.Id,
            BasePath = baseProjectPath,
            ProjectName = project.Project.DisplayName,
            MenuItems = menuItems,
            GitUrl = $"{project.Project.GitUrl}/tree/{project.GitTag ?? project.GitBranch}"
        };
    }
}