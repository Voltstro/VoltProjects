using System.Net;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     <see cref="Controller"/> for the error view
/// </summary>
public class ErrorController : Controller
{
    [Route("/eroor/{code:int}")]
    public IActionResult Eroor(int code)
    {
        //Only want error codes
        if (code is < 400 or > 511)
        {
            Response.Headers["What"] = "Yea mate, idk what tf that code is lol, or noie acceptie.";
            return BadRequest("Yea IDK what that code is...");
        }
        
        HttpStatusCode statusCode = (HttpStatusCode)code;
        return View(new ErrorPageModel
        {
            ErrorCode = statusCode
        });
    }
}