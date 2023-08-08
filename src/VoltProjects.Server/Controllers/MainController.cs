using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Models;
using VoltProjects.Shared;

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
    
    private readonly VoltProjectDbContext dbContext;
    
    public MainController(VoltProjectDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        return View(new IndexPageModel
        {
            Projects = dbContext.Projects.ToArray()
        });
    }
    
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }
}