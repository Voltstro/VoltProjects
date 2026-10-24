using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VoltProjects.Server.Controllers;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Shared.Models;
using VoltProjects.Tests.Core;
using VoltProjects.Tests.Utils;

namespace VoltProjects.Tests.Server.Controllers;

/// <summary>
///     Tests for <see cref="ErrorController"/>
/// </summary>
public class ErrorControllerTests : DbContextTest
{
    private ErrorController controller;
    
    public override void Init()
    {
        base.Init();
        
        ProjectService projectService = ServiceUtils.CreateProjectService(DbContext);
        ILogger<ErrorController> logger = new NullLogger<ErrorController>();
        controller = new ErrorController(projectService, logger)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    public override void Cleanup()
    {
        controller.Dispose();
        
        base.Cleanup();
    }

    /// <summary>
    ///     Does a bais 404 test with no project
    /// </summary>
    [Test]
    public async Task ErrorBasic404Test()
    {
        HttpStatusCode statusCode = HttpStatusCode.NotFound;
        IActionResult result = await controller.Eroor(CancellationToken.None, (int)statusCode);
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ErrorViewModel>());
        
        ErrorViewModel? errorViewModel = viewResult.Model as ErrorViewModel;
        Assert.That(errorViewModel, Is.Not.Null);
        Assert.That(errorViewModel.ErrorCode, Is.EqualTo(statusCode));
        Assert.That(errorViewModel.ProjectNavModel, Is.Null);
    }

    /// <summary>
    ///     Tests handling of error requests with invalid http code
    /// </summary>
    [Test]
    public async Task ErrorInvalidHttpCodeTest()
    {
        IActionResult result = await controller.Eroor(CancellationToken.None, 696969);
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ErrorViewModel>());
        
        ErrorViewModel? errorViewModel = viewResult.Model as ErrorViewModel;
        Assert.That(errorViewModel, Is.Not.Null);
        Assert.That(errorViewModel.ErrorCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(errorViewModel.ProjectNavModel, Is.Null);
    }

    /// <summary>
    ///     Test handling of error requests with a project
    /// </summary>
    [Test]
    public async Task ErrorWithProjectTest()
    {
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProjectMenuItem(project, ".", "Test1", 0);
        DbContext.CreateProjectMenuItem(project, "test2/", "Test2", 2);
        DbContext.CreateProjectMenuItem(project, "test3/", "Test3", 1);
        
        StatusCodeReExecuteFeature stateCodeFeature = new()
        {
            RouteValues = new RouteValueDictionary()
            {
                {"projectName", project.Project.Name},
                {"version", project.VersionTag}
            }
        };
        
        HttpStatusCode statusCode = HttpStatusCode.NotFound;
        controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(stateCodeFeature);
        IActionResult result;
        try
        {
            result = await controller.Eroor(CancellationToken.None, (int)statusCode);
        }
        finally
        {
            controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
        }
        
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ErrorViewModel>());
        
        ErrorViewModel? errorViewModel = viewResult.Model as ErrorViewModel;
        Assert.That(errorViewModel, Is.Not.Null);
        Assert.That(errorViewModel.ErrorCode, Is.EqualTo(statusCode));
        Assert.That(errorViewModel.ProjectNavModel, Is.Not.Null);
        
        Assert.That(errorViewModel.ProjectNavModel.ProjectName, Is.EqualTo(project.Project.Name));
        Assert.That(errorViewModel.ProjectNavModel.ProjectVersionTag, Is.EqualTo(project.VersionTag));
        Assert.That(errorViewModel.ProjectNavModel.MenuItems, Has.Count.EqualTo(3));
    }
    
    /// <summary>
    ///     Test handling of error requests with a project but the version is incorrect
    /// </summary>
    [Test]
    public async Task ErrorWithProjectInvalidVersionTest()
    {
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        
        StatusCodeReExecuteFeature stateCodeFeature = new()
        {
            RouteValues = new RouteValueDictionary()
            {
                {"projectName", project.Project.Name},
                {"version", "invalid"}
            }
        };
        
        HttpStatusCode statusCode = HttpStatusCode.NotFound;
        controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(stateCodeFeature);
        IActionResult result;
        try
        {
            result = await controller.Eroor(CancellationToken.None, (int)statusCode);
        }
        finally
        {
            controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
        }
        
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ErrorViewModel>());
        
        ErrorViewModel? errorViewModel = viewResult.Model as ErrorViewModel;
        Assert.That(errorViewModel, Is.Not.Null);
        Assert.That(errorViewModel.ErrorCode, Is.EqualTo(statusCode));
        Assert.That(errorViewModel.ProjectNavModel, Is.Not.Null);
        
        Assert.That(errorViewModel.ProjectNavModel.ProjectName, Is.EqualTo(project.Project.Name));
        Assert.That(errorViewModel.ProjectNavModel.ProjectVersionTag, Is.EqualTo(project.VersionTag));
    }
    
    /// <summary>
    ///     Test handling of error requests with an invalid project
    /// </summary>
    [Test]
    public async Task ErrorWithInvalidProjectTest()
    {
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        
        StatusCodeReExecuteFeature stateCodeFeature = new()
        {
            RouteValues = new RouteValueDictionary()
            {
                {"projectName", "Invalid"},
                {"version", "Invalid"}
            }
        };
        
        HttpStatusCode statusCode = HttpStatusCode.NotFound;
        controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(stateCodeFeature);
        IActionResult result;
        try
        {
            result = await controller.Eroor(CancellationToken.None, (int)statusCode);
        }
        finally
        {
            controller.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
        }
        
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ErrorViewModel>());
        
        ErrorViewModel? errorViewModel = viewResult.Model as ErrorViewModel;
        Assert.That(errorViewModel, Is.Not.Null);
        Assert.That(errorViewModel.ErrorCode, Is.EqualTo(statusCode));
        Assert.That(errorViewModel.ProjectNavModel, Is.Null);
    }
}