using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Core.SiteCache;
using VoltProjects.Server.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
public class MainController : Controller
{
    private readonly IndexPageModel _pageModel;
    
    public MainController(SiteCacheManager cacheManager)
    {
        _pageModel = new IndexPageModel()
        {
            Projects = cacheManager.ConfiguredProjects.ToArray()
        };
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        return View(_pageModel);
    }
    
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }
}