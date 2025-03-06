using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Telemetry;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     <see cref="Controller"/> for the error view
/// </summary>
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly ILogger<ErrorController> logger;

    public ErrorController(VoltProjectDbContext dbContext, ILogger<ErrorController> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }
    
    [Route("/eroor/{code:int}")]
    public async Task<IActionResult> Eroor(CancellationToken cancellationToken, int code)
    {
        HttpStatusCode statusCode = (HttpStatusCode)code;
        string errorMessage = "An unknown error has occured!";
        string errorMessageDetailed = "Sorry about that! Please try again later!";
        Project? project = null;
        ProjectVersion? projectVersion = null;
        ProjectNavModel? navModel = null;

        //Don't need bad shit happening on the error page
        try
        {
            //404 gets custom error code
            if (statusCode == HttpStatusCode.NotFound)
            {
                errorMessage = "That file doesn't exist!";
                errorMessageDetailed = "Sorry, but I don't have your request file on hand!";
            }
            
            //Get some details first
            IStatusCodeReExecuteFeature? statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature?.RouteValues != null)
            {
                //We have a project
                if (statusCodeReExecuteFeature.RouteValues.TryGetValue("projectName", out object? projectName))
                {
                    if (projectName is string projectNameString)
                    {
                        project = await dbContext.Projects
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Name == projectNameString, cancellationToken);
                    }
                    else
                    {
                        logger.LogWarning("Got a project name from route values that was either null or not a string!");
                    }
                    
                    //Project is not real
                    if (project == null)
                    {
                        errorMessage = "No such project!";
                        errorMessageDetailed = $"There is no project called '{projectName}' on hand!";
                    }
                }
                
                if (project != null && statusCodeReExecuteFeature.RouteValues.TryGetValue("version", out object? version))
                {
                    if (version is string versionString)
                    {
                        projectVersion = await dbContext.ProjectVersions
                            .AsNoTracking()
                            .Include(x => x.MenuItems!.OrderBy(j => j.ItemOrder))
                            .FirstOrDefaultAsync(x =>
                                x.ProjectId == project.Id
                                && x.VersionTag == versionString, cancellationToken: cancellationToken);

                        //If still null, then get default project version
                        if(projectVersion == null)
                            await GetProjectDefaultVersion(project, cancellationToken);
                    }
                    else
                    {
                        logger.LogWarning("Got a version from route values that was either null or not a string!");
                    }
                }
                else if(project != null) //No project version? Use default
                    projectVersion = await GetProjectDefaultVersion(project, cancellationToken);

                //if a project version exists, then so does it menu
                if (projectVersion is { MenuItems: not null })
                {
                    projectVersion.Project = project!;
                    string baseProjectPath = $"/{Path.Combine(project!.Name, projectVersion.VersionTag)}";
                    
                    using (Tracking.StartActivity(ActivityArea.Project, "nav"))
                    {
                        ProjectMenuItem[] menuItems = projectVersion.MenuItems.ToArray();
                        MenuItem[] builtMenuItems = new MenuItem[menuItems.Length];
                        for (int i = 0; i < builtMenuItems.Length; i++)
                        {
                            string menuPagePath = menuItems[i].Href;
                            builtMenuItems[i] = new MenuItem
                            {
                                Title = menuItems[i].Title,
                                Href = Path.Combine(baseProjectPath, menuPagePath),
                                IsActive = false
                            };
                        }

                        navModel = new ProjectNavModel
                        {
                            ProjectId = project.Id,
                            ProjectName = project.DisplayName,
                            BasePath = baseProjectPath,
                            GitUrl = $"{project.GitUrl}/tree/{projectVersion.GitTag ?? projectVersion.GitBranch}",
                            //MenuItems = builtMenuItems
                        };
                    }
                }

                //There was a file path lookup, display it to look nice
                if (project != null && statusCodeReExecuteFeature.RouteValues.TryGetValue("fullPath", out object? originalPath))
                    errorMessageDetailed = $"No file can be found at '{originalPath}'!";
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error has occured internally on the error page! THIS REALLY SHOULD NOT HAPPEN!");
            
            //Default everything
            errorMessage = "An unknown error has occured!";
            errorMessageDetailed = "Sorry about that! Please try again later!";
            project = null;
            projectVersion = null;
            navModel = null;
        }

        return View(new ErrorViewModel
        {
            ErrorCode = statusCode,
            ErrorMessage = errorMessage,
            ErrorMessageDetailed = errorMessageDetailed,
            Project = project?.Name,
            ProjectVersion = projectVersion?.VersionTag,
            ProjectNavModel = navModel
        });
    }

    private async Task<ProjectVersion?> GetProjectDefaultVersion(Project project, CancellationToken cancellationToken)
    {
        ProjectVersion? foundProjectVersion = await dbContext.ProjectVersions
            .AsNoTracking()
            .Include(x => x.MenuItems!.OrderBy(j => j.ItemOrder))
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.IsDefault, cancellationToken);
        
        return foundProjectVersion;
    }
}