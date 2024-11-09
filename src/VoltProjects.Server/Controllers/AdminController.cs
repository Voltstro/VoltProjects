using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models.View.Admin;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Services;
using VoltProjects.Shared.Services.Storage;

namespace VoltProjects.Server.Controllers;

[Route("/admin/")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
[Authorize]
public class AdminController : Controller
{
    private readonly VoltProjectDbContext dbContext;
    private readonly VoltProjectsConfig config;
    private readonly IStorageService storageService;
    private readonly ILogger<AdminController> logger;
    
    public AdminController(VoltProjectDbContext dbContext, IOptions<VoltProjectsConfig> config, IStorageService storageService, ILogger<AdminController> logger)
    {
        this.dbContext = dbContext;
        this.config = config.Value;
        this.storageService = storageService;
        this.logger = logger;
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
    [Route("projects/new", Name = "new")]
    [Route("projects/edit/{id:int}")]
    public IActionResult Project(int? id, bool? success)
    {
        //Fetch project (if an ID was passed in)
        ProjectPageModel? model = null;
        if (id != null)
        {
            Project? foundProject = dbContext.Projects.FirstOrDefault(x => x.Id == id);
            
            //if no project was found, goto new
            if(foundProject == null)
                return RedirectToAction("Project");

            if(foundProject.IconPath != null)
                foundProject.IconPath = Path.Combine(config.PublicUrl, foundProject.Name, foundProject.IconPath);
            model = new ProjectPageModel(foundProject)
            {
                Success = success
            };
        }
        
        return View(model);
    }

    [HttpPost]
    [Route("projects/edit/")]
    public async Task<IActionResult> Project(ProjectPageModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            Project? editProject;
            if (model.Id == null)
                editProject = new Project
                {
                    IconPath = null
                };
            else
            {
                editProject = dbContext.Projects.FirstOrDefault(x => x.Id == model.Id);
                if (editProject == null)
                    return NotFound();
            }
            
            editProject.Name = model.Name;
            editProject.DisplayName = model.DisplayName;
            editProject.ShortName = model.ShortName;
            editProject.Description = model.Description;
            editProject.GitUrl = model.GitUrl;

            if (model.UploadFile != null)
            {
                string fileName = Path.GetFileName(model.UploadFile.FileName);
                Stream fileStream = model.UploadFile.OpenReadStream();
                string contentType = MimeMap.GetMimeType(fileName);
                string uploadPath = Path.Combine(editProject.Name, fileName);

                GenericUploadFile uploadFile = new(fileStream, contentType, uploadPath);
                await storageService.UploadFileAsync(uploadFile);

                editProject.IconPath = fileName;

                await fileStream.DisposeAsync();
            }

            //New Project
            if (model.Id == null)
            {
                await dbContext.Projects.AddAsync(editProject);
            }
            
            await dbContext.SaveChangesAsync();
            model.Id = editProject.Id;
            model.Success = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save project.");
            model.Success = false;
        }

        return RedirectToAction("Project", new { id = model.Id, success = model.Success });
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