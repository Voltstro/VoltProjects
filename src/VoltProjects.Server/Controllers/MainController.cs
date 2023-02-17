using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using VoltProjects.Server.Models;
using VoltProjects.Server.Services.DocsServer;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
#if !DEBUG
[ResponseCache(Duration = 1209600)]
#endif
public class MainController : Controller
{
    //private readonly IndexPageModel _pageModel;

    private readonly DocsServerService serverService;
    
    public MainController(DocsServerService serverService)
    {
        this.serverService = serverService;
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        return View(new IndexPageModel
        {
            Projects = Array.Empty<Project>()
        });
    }
    
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }

    [Route("/{project}/{**catchAll}")]
    public IActionResult Main(string project, string? catchAll)
    {
        return RedirectToAction("Main", new {project = project, version = "latest", catchAll = catchAll});
    }

    [Route("/{project}/{version}/{**catchAll}")]
    public IActionResult Main(string project, string version, string? catchAll)
    {
        IActionResult? content = serverService.TryGetProjectFile(Request, ViewData, project, version, catchAll);
        if (content != null)
            return content;
        
        return NotFound();
    }
}