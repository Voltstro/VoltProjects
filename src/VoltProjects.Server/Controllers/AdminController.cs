using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
    [Route("projects/edit/")]
    public IActionResult Project()
    {
        return RedirectToRoute("new");
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
            Project? foundProject = dbContext.Projects
                .Include(x => x.ProjectVersions)
                .ThenInclude(x => x.DocBuilder)
                .FirstOrDefault(x => x.Id == id);
            
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
        {
            if (model.Id != null)
            {
                //Get project versions again
                Project? foundProject = dbContext.Projects
                    .Include(x => x.ProjectVersions)
                    .ThenInclude(x => x.DocBuilder)
                    .FirstOrDefault(x => x.Id == model.Id);

                model.ProjectVersions = foundProject?.ProjectVersions;
                model.LastUpdateTime = foundProject?.LastUpdateTime ?? DateTime.UtcNow;
                model.CreationTime = foundProject?.CreationTime ?? DateTime.UtcNow;
            }
            
            return View(model);
        }

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
                await dbContext.Projects.AddAsync(editProject);
            
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
    [Route("projects/edit/{projectId:int}/versions/new/")]
    [Route("projects/edit/{projectId:int}/versions/edit/{projectVersionId:int}", Name = "AdminProjectVersionEdit")]
    public async Task<IActionResult> ProjectVersion(int projectId, int? projectVersionId, bool? success)
    {
        ProjectVersionPageModel? model;

        DocBuilder[] docBuilders = await dbContext.DocBuilders.ToArrayAsync();
        Language[] languages = await dbContext.Languages.ToArrayAsync();
        
        if (projectVersionId != null)
        {
            ProjectVersion? foundProjectVersion = await dbContext.ProjectVersions
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == projectVersionId);

            if (foundProjectVersion == null)
                return NotFound();

            //Use our already fetched details for filling the virtuals so the view can have the details
            foundProjectVersion.DocBuilder = docBuilders.First(x => x.Id == foundProjectVersion.DocBuilderId);
            foundProjectVersion.Language = languages.First(x => x.Id == foundProjectVersion.LanguageId);

            model = new ProjectVersionPageModel(foundProjectVersion);
        }
        else
        {
            model = new ProjectVersionPageModel
            {
                ProjectId = projectId
            };
        }

        model.DocBuilders = docBuilders;
        model.Languages = languages;
        model.Success = success;
        
        return View(model);
    }

    [HttpPost]
    [Route("projects/edit/{projectId:int}/versions/", Name = "ProjectVersionsPost")]
    public async Task<IActionResult> ProjectVersion(int projectId, ProjectVersionPageModel model)
    {
        ModelState.Remove($"{nameof(ProjectVersionPageModel.Project)}");
        ModelState.Remove($"{nameof(ProjectVersionPageModel.DocBuilder)}");
        ModelState.Remove($"{nameof(ProjectVersionPageModel.Language)}");
        
        ModelState.Remove($"{nameof(ProjectVersionPageModel.DocBuilders)}");
        ModelState.Remove($"{nameof(ProjectVersionPageModel.Languages)}");
        
        if (!ModelState.IsValid)
        {
            DocBuilder[] docBuilders = await dbContext.DocBuilders.ToArrayAsync();
            Language[] languages = await dbContext.Languages.ToArrayAsync();
            model.DocBuilders = docBuilders;
            model.Languages = languages;
            return View(model);
        }

        try
        {
            ProjectVersion? editProject;
            if (model.Id == null)
                editProject = new ProjectVersion
                {
                    ProjectId = projectId
                };
            else
            {
                editProject = dbContext.ProjectVersions.FirstOrDefault(x => x.Id == model.Id);
                if (editProject == null)
                    return NotFound();
            }

            editProject.VersionTag = model.VersionTag;
            editProject.GitBranch = model.GitBranch;
            editProject.DocsPath = model.DocsPath;
            editProject.DocsBuiltPath = model.DocsBuiltPath;
            editProject.DocBuilderId = model.DocBuilderId;
            editProject.LanguageId = model.LanguageId;
            editProject.IsDefault = model.IsDefault;
            
            //New Project
            if (model.Id == null)
                await dbContext.ProjectVersions.AddAsync(editProject);

            await dbContext.SaveChangesAsync();
            //model.Id = editProject.Id;
            model.Success = true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to save project version.");
            model.Success = false;
        }

        return RedirectToAction("ProjectVersion", "Admin",
            new { projectId = model.ProjectId, projectVersionId = model.Id, success = model.Success });
        //return RedirectToRoute("AdminProjectVersionEdit",  new { projectId = model.ProjectId, projectVersionId = model.Id, success = model.Success });
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