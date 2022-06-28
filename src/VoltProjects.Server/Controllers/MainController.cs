using Microsoft.AspNetCore.Mvc;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
public class MainController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }
}