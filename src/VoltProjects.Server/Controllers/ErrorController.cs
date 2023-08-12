using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     <see cref="Controller"/> for the error view
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly ProjectMenuService projectMenuService;
    private readonly ILogger<ErrorController> logger;

    public ErrorController(VoltProjectDbContext dbContext, ProjectMenuService projectMenuService, ILogger<ErrorController> logger)
    {
        this.dbContext = dbContext;
        this.projectMenuService = projectMenuService;
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
                    project = await dbContext.Projects
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Name == (string)projectName, cancellationToken);

                    //Project is not real
                    if (project == null)
                    {
                        errorMessage = "No such project!";
                        errorMessageDetailed = $"There is no project called '{projectName}' on hand!";
                    }
                }
                
                if (project != null && statusCodeReExecuteFeature.RouteValues.TryGetValue("version", out object? version))
                {
                    //Lets check if the project version is REALLLLL
                    projectVersion = await dbContext.ProjectVersions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.ProjectId == project.Id && x.VersionTag == (string)version, cancellationToken: cancellationToken) ??
                                     await GetProjectDefaultVersion(project, cancellationToken); //If project version is null, use default
                }
                else if(project != null) //No project version? Use default
                    projectVersion = await GetProjectDefaultVersion(project, cancellationToken);

                //if a project version exists, then so does a project, and so does a menu
                if (projectVersion != null)
                {
                    projectVersion.Project = project!;
                    string baseProjectPath = $"/{Path.Combine(project!.Name, projectVersion.VersionTag)}";
                    navModel = await projectMenuService.GetProjectMenu(string.Empty, baseProjectPath, projectVersion,
                        cancellationToken);
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
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.IsDefault, cancellationToken);
        
        return foundProjectVersion;
    }
}