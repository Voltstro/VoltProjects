using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VoltProjects.Server.Controllers;

[Route("/admin/auth/")]
public class AdminAuthController : Controller
{
    [HttpGet]
    [Route("signout")]
    public IActionResult Signout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }

    [HttpGet]
    [Route("profile")]
    [Authorize]
    public IActionResult UserProfile()
    {
        string? picture = User.FindFirstValue("picture");
        if (!string.IsNullOrEmpty(picture))
            return Redirect(picture);
        
        string defaultUserProfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/defaultuser.webp");
        string mimeType = "image/webp";
        FileStream fileStream = new(defaultUserProfile, FileMode.Open);
        
        return File(fileStream, mimeType);
    }
}