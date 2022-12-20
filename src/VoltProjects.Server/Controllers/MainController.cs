using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Models;
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
    
    public MainController()
    {
        /*
        _pageModel = new IndexPageModel()
        {
            Projects = cacheManager.ConfiguredProjects.ToArray()
        };
        */
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

    [Route("/{**catchAll}")]
    public IActionResult Main(string catchAll)
    {
        Debug.WriteLine(catchAll);
        string? route = RouteData.Values["page"] as string;

        return NotFound();
    }
}