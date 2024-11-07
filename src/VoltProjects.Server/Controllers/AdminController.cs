using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models.View.Admin;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

[Route("/admin/")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Authorize]
public class AdminController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly VoltProjectsConfig config;
    
    public AdminController(VoltProjectDbContext dbContext, IOptions<VoltProjectsConfig> config)
    {
        this.dbContext = dbContext;
        this.config = config.Value;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("projects")]
    public IActionResult Projects(int page, int size)
    {
        //Size should either be 10, 25 or 50
        if (size != 5 && size != 10 && size != 25 && size != 50)
            size = 10;
        
        //Page should always be 1 or more
        page = page <= 0 ? 1 : page;
        
        Project[] projects = dbContext.Projects
            .Skip((page - 1) * size)
            .Take(size)
            .ToArray();
        
        return View(new ProjectsPageModel
        {
            Projects = projects
        });
    }

    [HttpGet]
    [Route("projects/new")]
    [Route("projects/edit/{id:int}")]
    public IActionResult Project(int? id, bool success)
    {
        ProjectPageModel? project = null;
        
        //Fetch project (if an ID was passed in)
        if (id != null)
        {
            Project? foundProject = dbContext.Projects.FirstOrDefault(x => x.Id == id);
            
            //if no project was found, goto new
            if(foundProject == null)
                return RedirectToAction("Project");

            foundProject.IconPath = Path.Combine(config.PublicUrl, foundProject.Name, foundProject.IconPath);
            project = new ProjectPageModel(foundProject);
        }

        if (success && project != null)
            project.Success = true;
        
        return View(project);
    }

    [HttpPost]
    [Route("projects/edit/")]
    public IActionResult Project(ProjectPageModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        //New Project
        if (model.Id == null)
        {
            return RedirectToAction("Project", model.Id);
        }
        
        //Edit project
        Project? editProject = dbContext.Projects.FirstOrDefault(x => x.Id == model.Id);
        if (editProject == null)
            return NotFound();

        editProject.Name = model.Name;
        editProject.DisplayName = model.DisplayName;
        editProject.ShortName = model.ShortName;
        editProject.Description = model.Description;
        editProject.GitUrl = model.GitUrl;
        dbContext.SaveChanges();
        
        return RedirectToAction("Project", new { id = editProject.Id, success = true });
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