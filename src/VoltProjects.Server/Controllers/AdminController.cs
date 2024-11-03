using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoltProjects.Server.Controllers;

[Route("/admin/")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Authorize]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
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