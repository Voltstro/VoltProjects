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
            Projects = Array.Empty<VoltProject>()
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
        Debug.WriteLine(catchAll);

        ContentResult? content = serverService.TryGetProjectFile(project, catchAll);
        if (content != null)
            return content;
        
        return NotFound();
    }
}