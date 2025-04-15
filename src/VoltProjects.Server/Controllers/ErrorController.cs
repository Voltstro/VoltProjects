using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Telemetry;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     <see cref="Controller"/> for the error view
/// </summary>
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorController : Controller
{
    private readonly ProjectService projectService;
    private readonly ILogger<ErrorController> logger;

    public ErrorController(ProjectService projectService, ILogger<ErrorController> logger)
    {
        this.projectService = projectService;
        this.logger = logger;
    }
    
    [Route("/eroor/{code:int}")]
    public async Task<IActionResult> Eroor(CancellationToken cancellationToken, int code)
    {
        HttpStatusCode statusCode = (HttpStatusCode)code;
        string errorMessage = "An unknown error has occured!";
        string errorMessageDetailed = "Sorry about that! Please try again later!";

        ProjectVersion? projectVersion = null;
        ProjectNavModel? navModel = null;

        //Don't need bad shit happening on the error page
        try
        {
            //404 gets custom error code
            if (statusCode == HttpStatusCode.NotFound)
            {
                errorMessage = "That file doesn't exist!";
                errorMessageDetailed = "The requested file doesn't exist!";
            }
            
            //Get some details first
            IStatusCodeReExecuteFeature statusCodeReExecuteFeature = HttpContext.Features.GetRequiredFeature<IStatusCodeReExecuteFeature>();
            RouteValueDictionary? routeValues = statusCodeReExecuteFeature.RouteValues;
            object? projectNameObj = null;
            if (routeValues != null 
                && routeValues.TryGetValue("projectName", out projectNameObj) 
                && routeValues.TryGetValue("version", out object? versionObj)
                && projectNameObj is string projectName
                && versionObj is string version)
            {
                //Fetch project version
                //If the full request comes back null, then attempt to get the default project
                projectVersion = await projectService.GetProjectVersion(projectName, version) ?? await projectService.GetProjectDefaultVersion(projectName);
            }
            
            //There was a requested project, but it doesn't exist
            if (projectNameObj != null && projectVersion == null)
            {
                errorMessage = "That project doesn't exist!";
                errorMessageDetailed = "The requested project doesn't exist!";
            }
            else if (projectVersion != null)
            {
                using (Tracking.StartActivity(ActivityArea.Project, "nav"))
                {
                    IReadOnlyList<MenuItem> menuItems = await projectService.GetProjectMenuItems(projectVersion, null);
                    navModel = new ProjectNavModel(projectVersion, menuItems);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error has occured internally on the error page! THIS REALLY SHOULD NOT HAPPEN!");
            
            //Default everything
            errorMessage = "An unknown error has occured!";
            errorMessageDetailed = "Sorry about that! Please try again later!";
            navModel = null;
        }

        return View(new ErrorViewModel
        {
            ErrorCode = statusCode,
            ErrorMessage = errorMessage,
            ErrorMessageDetailed = errorMessageDetailed,
            ProjectNavModel = navModel
        });
    }
}