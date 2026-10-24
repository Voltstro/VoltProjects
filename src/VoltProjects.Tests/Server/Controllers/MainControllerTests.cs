using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using VoltProjects.Server.Controllers;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Tests.Core;
using VoltProjects.Tests.Utils;

namespace VoltProjects.Tests.Server.Controllers;

/// <summary>
///     Tests for <see cref="MainController"/>
/// </summary>
public class MainControllerTests : DbContextTest
{
    private MainController controller;
    
    public override void Init()
    {
        base.Init();
        
        ProjectService projectService = ServiceUtils.CreateProjectService(DbContext);
        IOptions<VoltProjectsConfig> options = ConfigUtils.CreateServerConfig();
        controller = new MainController(projectService, options);
    }

    public override void Cleanup()
    {
        controller.Dispose();
        
        base.Cleanup();
    }

    /// <summary>
    ///     Tests index page
    /// </summary>
    [Test]
    public async Task IndexTest()
    {
        //Setup setup data
        DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProject("Test2", "Test2 Project", "latest");
        
        //Call index and test
        IActionResult result = await controller.Index();
        
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<MainViewModel>());
        
        MainViewModel? mainViewModel = viewResult.Model as MainViewModel;
        Assert.That(mainViewModel, Is.Not.Null);
        Assert.That(mainViewModel.Projects, Has.Length.EqualTo(2));
    }

    /// <summary>
    ///     Test for about page
    /// </summary>
    [Test]
    public void AboutTest()
    {
        //Call about and test
        IActionResult result = controller.About();
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<AboutViewModel>());
        
        AboutViewModel? aboutViewModel = viewResult.Model as AboutViewModel;
        Assert.That(aboutViewModel, Is.Not.Null);
        Assert.That(aboutViewModel.Projects, Has.Length.GreaterThan(0));
        Assert.That(aboutViewModel.Developers, Has.Length.GreaterThan(0));
    }
}