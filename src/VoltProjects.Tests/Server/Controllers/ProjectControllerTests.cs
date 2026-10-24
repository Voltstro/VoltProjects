using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using VoltProjects.Server.Controllers;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Tests.Core;
using VoltProjects.Tests.Utils;

namespace VoltProjects.Tests.Server.Controllers;

/// <summary>
///     Tests for <see cref="ProjectController"/>
/// </summary>
public class ProjectControllerTests : DbContextTest
{
    private ProjectController controller;

    public override void Init()
    {
        base.Init();
        
        ProjectService projectService = ServiceUtils.CreateProjectService(DbContext);
        IOptions<VoltProjectsConfig> options = ConfigUtils.CreateServerConfig();
        controller = new ProjectController(projectService, options);
    }

    public override void Cleanup()
    {
        controller.Dispose();
        
        base.Cleanup();
    }

    /// <summary>
    ///     Basic project view test getting a simple page
    /// </summary>
    [Test]
    public async Task ProjectViewPageTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProjectPage(project, ".", "Test", "Test Page.");

        //Do action and assert results
        IActionResult result = await controller.ProjectView("Test1", "latest", null);
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ProjectViewModel>());
        
        ProjectViewModel? projectViewModel = viewResult.Model as ProjectViewModel;
        Assert.That(projectViewModel, Is.Not.Null);
        
        Assert.That(projectViewModel.ProjectPage.Title, Is.EqualTo("Test"));
    }
    
    /// <summary>
    ///     Test for a project page with a TOC
    /// </summary>
    [Test]
    public async Task ProjectViewPageWithTocTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        ProjectPage projectPage = DbContext.CreateProjectPage(project, "test/", "Test", "Test Page.");
        
        ProjectTocItem[] tocItems =
        [
            new() { ItemOrder = 0, Title = "Test1", Href = "test1/" },
            new() { ItemOrder = 1, Title = "Test2", Href = "test2/" },
            new() { ItemOrder = 2, Title = "Test3" },
            new() { ItemOrder = 4, Title = "Test4" },
        ];
        ProjectToc projectToc = DbContext.CreateProjectToc(projectPage, tocItems);
        
        //Create child to test3
        DbContext.CreateProjectTocItem(projectToc, new ProjectTocItem { ItemOrder = 3, Title = "Test3Child", Href = "test3/child/", ParentTocItem = tocItems[2] });
        
        //Create child to test4
        DbContext.CreateProjectTocItem(projectToc, new ProjectTocItem { ItemOrder = 5, Title = "Test4Child", ParentTocItem = tocItems[3] });

        //Do action and assert results
        IActionResult result = await controller.ProjectView("Test1", "latest", "test/");
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ProjectViewModel>());
        
        ProjectViewModel? projectViewModel = viewResult.Model as ProjectViewModel;
        Assert.That(projectViewModel, Is.Not.Null);
        
        Assert.That(projectViewModel.ProjectPage.Title, Is.EqualTo("Test"));
        
        //Correct number of toc items
        Assert.That(projectViewModel.TocItems, Is.Not.Null);
        Assert.That(projectViewModel.TocItems, Has.Count.EqualTo(4));
    }
    
    /// <summary>
    ///     Test for a project page with a menu
    /// </summary>
    [Test]
    public async Task ProjectViewPageWithMenuTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test", "Test Project", "latest");
        DbContext.CreateProjectPage(project, ".", "Test", "Test Page.");
        DbContext.CreateProjectMenuItem(project, ".", "Test1", 0);
        DbContext.CreateProjectMenuItem(project, "test3/", "Test3", 2);
        DbContext.CreateProjectMenuItem(project, "test2/", "Test2", 1);
        
        //Do action and assert results
        IActionResult result = await controller.ProjectView("Test", "latest", null);
        Assert.That(result, Is.TypeOf<ViewResult>());
        ViewResult? viewResult = result as ViewResult;
        
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.TypeOf<ProjectViewModel>());
        
        ProjectViewModel? projectViewModel = viewResult.Model as ProjectViewModel;
        Assert.That(projectViewModel, Is.Not.Null);
        
        Assert.That(projectViewModel.ProjectNavModel, Is.Not.Null);
        Assert.That(projectViewModel.ProjectNavModel.MenuItems, Is.Not.Null);
        Assert.That(projectViewModel.ProjectNavModel.MenuItems, Has.Count.EqualTo(3));
        Assert.That(projectViewModel.ProjectNavModel.MenuItems[0].Title, Is.EqualTo("Test1"));
        Assert.That(projectViewModel.ProjectNavModel.MenuItems[1].Title, Is.EqualTo("Test2")); //Order should make test2 second returned
    }

    /// <summary>
    ///     Test when the project is valid, but the page is simply not found
    /// </summary>
    [Test]
    public async Task ProjectViewNotFoundTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProjectPage(project, ".", "Test", "Test Page.");
        
        //Do action and assert results
        IActionResult result = await controller.ProjectView("Test1", "latest", "test/path");
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    /// <summary>
    ///     Test when no project version is provided that it redirects to the default version
    /// </summary>
    [Test]
    public async Task ProjectViewNoProjectVersionRedirectsToDefaultProjectTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProjectPage(project, ".", "Test", "Test Page.");

        //Do action and assert results
        IActionResult result = await controller.ProjectView("Test1", null, null);
        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        RedirectToActionResult? redirectToActionResult = result as RedirectToActionResult;
        
        Assert.That(redirectToActionResult, Is.Not.Null);
        Assert.That(redirectToActionResult.RouteValues, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(redirectToActionResult.RouteValues["version"], Is.EqualTo("latest"));
            Assert.That(redirectToActionResult.RouteValues["projectName"], Is.EqualTo("Test1"));
            Assert.That(redirectToActionResult.RouteValues["fullPath"], Is.Null);
        }
    }
    

    /// <summary>
    ///     Test of an invalid project version getting redirected
    /// </summary>
    [Test]
    public async Task ProjectViewInvalidProjectVersionRedirectsToDefaultProjectTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test1", "Test1 Project", "latest");
        DbContext.CreateProjectPage(project, ".", "Test", "Test Page.");

        //Do action and asset results
        IActionResult result = await controller.ProjectView("Test1", "notvalid", "test/path/");
        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        RedirectToActionResult? redirectToActionResult = result as RedirectToActionResult;
        
        Assert.That(redirectToActionResult, Is.Not.Null);
        Assert.That(redirectToActionResult.RouteValues, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(redirectToActionResult.RouteValues["version"], Is.EqualTo("latest"));
            Assert.That(redirectToActionResult.RouteValues["projectName"], Is.EqualTo("Test1"));
            Assert.That(redirectToActionResult.RouteValues["fullPath"], Is.EqualTo("notvalid/test/path/"));
        }
    }
}