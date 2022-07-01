using Microsoft.AspNetCore.Mvc;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
public class MainController : Controller
{
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }
    
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }
}