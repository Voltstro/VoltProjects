using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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

    private IContentTypeProvider typeProvider;
    
    public MainController()
    {
        typeProvider = new FileExtensionContentTypeProvider();
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

        //TODO: Prober solution for this lol
        //Find with extension
        string fullSitePath = Path.Join(AppContext.BaseDirectory, "Sites/", catchAll);
        
        if (System.IO.File.Exists(fullSitePath))
        {
            string? fileMimeType = null;
            if (typeProvider.TryGetContentType(fullSitePath, out string? type))
            {
                fileMimeType = type;
            }
            
            return Content(System.IO.File.ReadAllText(fullSitePath), fileMimeType);
        }
            
        
        return NotFound();
    }
}