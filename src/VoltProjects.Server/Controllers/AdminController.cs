using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models.View.Admin;
using VoltProjects.Server.Shared;
using VoltProjects.Server.Shared.Paging;
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

    #region Projects

    [HttpGet]
    [Route("projects/")]
    public async Task<IActionResult> Projects(int page, int size)
    {
        IQueryable<Project> projectsQuery = dbContext.Projects.OrderByDescending(x => x.Id);
        PagedResult<Project> projectsPaged = await PagedResult<Project>.Create(projectsQuery, page, size);
        
        return View(new ProjectsPageModel
        {
            Projects = projectsPaged
        });
    }
    
    [HttpGet]
    [Route("projects/edit/")]
    public IActionResult Project()
    {
        return RedirectToRoute("new");
    }

    [HttpGet]
    [Route("projects/new/", Name = "AdminProjectNew")]
    [Route("projects/edit/{id:int}")]
    public IActionResult Project(int? id, bool? success)
    {
        //Fetch project (if an ID was passed in)
        ProjectPageModel? model = null;
        if (id != null)
        {
            Project? foundProject = dbContext.Projects
                .Include(x => x.ProjectVersions.OrderByDescending(y => y.Id))
                .ThenInclude(x => x.DocBuilder)
                .FirstOrDefault(x => x.Id == id);
            
            //if no project was found, goto new
            if (foundProject == null)
                return NotFound();

            if(foundProject.IconPath != null)
                foundProject.IconPath = Path.Combine(config.PublicUrl, foundProject.Name, foundProject.IconPath);
            model = new ProjectPageModel(foundProject)
            {
                Success = success
            };
        }
        else
        {
            model = new ProjectPageModel();
        }
        
        return View(model);
    }

    [HttpPost]
    [Route("projects/edit/")]
    [Route("projects/edit/{id:int}")]
    public async Task<IActionResult> Project(int? id, ProjectPageModel model)
    {
        if (!ModelState.IsValid)
        {
            if (id != null)
            {
                //Get project versions again
                Project? foundProject = dbContext.Projects
                    .Include(x => x.ProjectVersions)
                    .ThenInclude(x => x.DocBuilder)
                    .FirstOrDefault(x => x.Id == id);

                model.ProjectVersions = foundProject?.ProjectVersions;
                model.LastUpdateTime = foundProject?.LastUpdateTime ?? DateTime.UtcNow;
                model.CreationTime = foundProject?.CreationTime ?? DateTime.UtcNow;
            }
            
            return View(model);
        }

        try
        {
            Project? editProject;
            if (id == null)
                editProject = new Project
                {
                    IconPath = null
                };
            else
            {
                editProject = dbContext.Projects.FirstOrDefault(x => x.Id == id);
                if (editProject == null)
                    return NotFound();
            }
            
            editProject.Name = model.Name;
            editProject.DisplayName = model.DisplayName;
            editProject.ShortName = model.ShortName;
            editProject.Description = model.Description;
            editProject.GitUrl = model.GitUrl;
            editProject.Published = model.Published;

            if (model.ProjectIcon != null)
            {
                string fileName = Path.GetFileName(model.ProjectIcon.FileName);
                Stream fileStream = model.ProjectIcon.OpenReadStream();
                string contentType = MimeMap.GetMimeType(fileName);
                string uploadPath = Path.Combine(editProject.Name, fileName);

                GenericUploadFile uploadFile = new(fileStream, contentType, uploadPath);
                await storageService.UploadFileAsync(uploadFile);

                editProject.IconPath = fileName;

                await fileStream.DisposeAsync();
            }

            //New Project
            if (id == null)
                await dbContext.Projects.AddAsync(editProject);
            
            await dbContext.SaveChangesAsync();
            id = editProject.Id;
            model.Success = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save project.");
            model.Success = false;
        }

        return RedirectToAction("Project", new { id = id, success = model.Success });
    }

    [HttpGet]
    [Route("projects/edit/{projectId:int}/versions/new/", Name = "AdminProjectVersionNew")]
    [Route("projects/edit/{projectId:int}/versions/edit/{projectVersionId:int}")]
    public async Task<IActionResult> ProjectVersion(int projectId, int? projectVersionId, bool? success)
    {
        ProjectVersionPageModel? model;

        DocBuilder[] docBuilders = await dbContext.DocBuilders.ToArrayAsync();
        Language[] languages = await dbContext.Languages.ToArrayAsync();
        
        if (projectVersionId != null)
        {
            ProjectVersion? foundProjectVersion = await dbContext.ProjectVersions
                .Include(x => x.Project)
                .Include(x => x.PreBuilds.OrderBy(y => y.Order))
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
            Project project = dbContext.Projects
                .First(x => x.Id == projectId);
            model = new ProjectVersionPageModel
            {
                ProjectId = projectId,
                Project = project
            };
        }

        model.DocBuilders = docBuilders;
        model.Languages = languages;
        model.Success = success;
        
        return View(model);
    }

    [HttpPost]
    [Route("projects/edit/{projectId:int}/versions/edit/")]
    [Route("projects/edit/{projectId:int}/versions/edit/{projectVersionId:int}")]
    public async Task<IActionResult> ProjectVersion(int projectId, int? projectVersionId, ProjectVersionPageModel model)
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
            
            Project project = dbContext.Projects
                .First(x => x.Id == projectId);

            model.Id = projectVersionId;
            model.Project = project;
            model.DocBuilders = docBuilders;
            model.Languages = languages;
            model.Success = false;
            return View(model);
        }

        try
        {
            ProjectVersion? editProject;
            if (projectVersionId == null)
                editProject = new ProjectVersion
                {
                    ProjectId = projectId
                };
            else
            {
                editProject = dbContext.ProjectVersions.FirstOrDefault(x => x.Id == projectVersionId);
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
            editProject.SeIndexable = model.SeIndexable;
            editProject.Published = model.Published;
            
            //Pre-Builds
            if (model.PreBuildCommands != null)
            {
                List<ProjectPreBuildPageModel> preBuilds = new();
                foreach (ProjectPreBuildPageModel preBuild in model.PreBuildCommands)
                {
                    preBuild.ProjectVersion = editProject;

                    if (preBuild.Deleted)
                        dbContext.PreBuildCommands.Remove(preBuild);
                    else
                    {
                        preBuilds.Add(preBuild);
                    
                        //Ignore updating times
                        if (!preBuild.New)
                        {
                            dbContext.Entry(preBuild).State = EntityState.Modified;
                            dbContext.Entry(preBuild).Property(p => p.CreationTime).IsModified = false;
                            dbContext.Entry(preBuild).Property(p => p.LastUpdateTime).IsModified = false;
                        }
                    }
                }

                editProject.PreBuilds = preBuilds.ToArray();
            }
            
            //New Project
            if (projectVersionId == null)
                await dbContext.ProjectVersions.AddAsync(editProject);

            await dbContext.SaveChangesAsync();
            
            projectVersionId = editProject.Id;
            model.Success = true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to save project version.");
            model.Success = false;
        }

        return RedirectToAction("ProjectVersion", "Admin",
            new { projectId = model.ProjectId, projectVersionId = projectVersionId, success = model.Success });
    }
    
    #endregion

    #region Build Schedules

    [HttpGet]
    [Route("build/schedules/")]
    public async Task<IActionResult> BuildSchedules(int page, int size)
    {
        IQueryable<ProjectBuildSchedule> buildSchedulesQuery = dbContext
            .ProjectBuildSchedules
            .Include(x => x.ProjectVersion)
            .ThenInclude(x => x.Project)
            .OrderByDescending(x => x.Id);
        PagedResult<ProjectBuildSchedule> buildSchedules =
            await PagedResult<ProjectBuildSchedule>.Create(buildSchedulesQuery, page, size);
        
        return View(new BuildSchedulesModel
        {
            BuildSchedules = buildSchedules
        });
    }

    [HttpGet]
    [Route("build/schedules/new/", Name = "AdminBuildScheduleNew")]
    [Route("build/schedules/edit/{buildScheduleId:int}/")]
    public IActionResult BuildSchedule(int? buildScheduleId, bool? success)
    {
        //Get all project versions
        ProjectVersion[] projectVersions = dbContext.ProjectVersions
            .Include(x => x.Project)
            .ToArray();
        
        BuildScheduleModel viewModel;
        if (buildScheduleId != null)
        {
            ProjectBuildSchedule? buildSchedule =
                dbContext.ProjectBuildSchedules.FirstOrDefault(x => x.Id == buildScheduleId);

            if (buildSchedule == null)
                return NotFound();

            viewModel = new BuildScheduleModel(buildSchedule);
            viewModel.ProjectVersion = projectVersions.First(x => x.Id == viewModel.ProjectVersionId);
        }
        else
        {
            viewModel = new BuildScheduleModel();
        }
        
        viewModel.ProjectVersions = projectVersions;
        viewModel.Success = success;
        
        return View(viewModel);
    }

    [HttpPost]
    [Route("build/schedules/edit/")]
    [Route("build/schedules/edit/{buildScheduleId:int}/")]
    public IActionResult BuildSchedule(int? buildScheduleId, BuildScheduleModel model)
    {
        ModelState.Remove(nameof(BuildScheduleModel.ProjectVersion));
        ModelState.Remove(nameof(BuildScheduleModel.ProjectVersions));
        
        if (!ModelState.IsValid)
        {
            //Get project versions again
            ProjectVersion[] projectVersions = dbContext.ProjectVersions
                .Include(x => x.Project)
                .ToArray();
            
            //Get build schedules again
            ProjectBuildSchedule buildSchedule =
                dbContext.ProjectBuildSchedules.First(x => x.Id == buildScheduleId);
            
            model.ProjectVersion = projectVersions.First(x => x.Id == buildSchedule.ProjectVersionId);
            model.ProjectVersions = projectVersions;
            model.LastExecuteTime = buildSchedule.LastExecuteTime;
            model.LastUpdateTime = buildSchedule.LastUpdateTime;
            model.CreationTime = buildSchedule.CreationTime;
            return View(model);
        }

        try
        {
            ProjectBuildSchedule? buildSchedule;
            if (buildScheduleId == null)
                buildSchedule = new ProjectBuildSchedule();
            else
            {
                buildSchedule = dbContext.ProjectBuildSchedules.FirstOrDefault(x => x.Id == buildScheduleId);
                if (buildSchedule == null)
                    return NotFound();
            }

            buildSchedule.ProjectVersionId = model.ProjectVersionId;
            buildSchedule.Cron = model.Cron;
            buildSchedule.IsActive = model.IsActive;
            buildSchedule.IgnoreBuildEvents = model.IgnoreBuildEvents;

            if (buildScheduleId == null)
                dbContext.ProjectBuildSchedules.Add(buildSchedule);

            dbContext.SaveChanges();
            model.Success = true;
            buildScheduleId = buildSchedule.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save project build schedule.");
            model.Success = false;
        }
        
        return RedirectToAction("BuildSchedule", "Admin",
            new { buildScheduleId = buildScheduleId, success = model.Success });
    }

    #endregion

    #region Build Events

    [HttpGet]
    [Route("build/events/")]
    public async Task<IActionResult> BuildEvents(int page, int size)
    {
        IQueryable<ProjectBuildEvent> buildEventsQuery = dbContext.ProjectBuildEvents
            .Include(x => x.Project)
            .ThenInclude(x => x.Project)
            .OrderByDescending(x => x.Id);
        PagedResult<ProjectBuildEvent>
            buildEvents = await PagedResult<ProjectBuildEvent>.Create(buildEventsQuery, page, size);
        
        return View(new BuildEventsModel
        {
            ProjectBuildEvents = buildEvents
        });
    }

    [HttpGet]
    [Route("build/events/{eventId:int}")]
    public async Task<IActionResult> BuildEvent(int eventId, int page, int size)
    {
        ProjectBuildEvent? buildEvent = await dbContext.ProjectBuildEvents
            .Include(x => x.Project)
            .ThenInclude(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == eventId);
        
        if (buildEvent == null)
            return NotFound();

        IQueryable<ProjectBuildEventLog> buildEventLogsQuery = dbContext.ProjectBuildEventLogs
            .Where(x => x.BuildEventId == eventId)
            .Include(x => x.LogLevel)
            .OrderBy(x => x.Id);
        PagedResult<ProjectBuildEventLog> buildEventLogs = await PagedResult<ProjectBuildEventLog>.Create(
            buildEventLogsQuery, page, size, [45, 100, 150]);
        
        return View(new BuildEventModel(buildEvent, buildEventLogs));
    }

    #endregion
}