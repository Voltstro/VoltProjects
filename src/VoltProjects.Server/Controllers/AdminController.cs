using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

[Route("/admin/")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Authorize]
public class AdminController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    
    public AdminController(VoltProjectDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("projects")]
    public IActionResult Projects()
    {
        Project[] projects = dbContext.Projects.ToArray();
        
        return View();
    }

    [HttpGet]
    [Route("signout")]
    public IActionResult Signout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }
}