using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

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
        Project[] allProjects = dbContext.Projects.ToArray();
        ProjectInfo[] projectInfos = new ProjectInfo[allProjects.Length];
        for (int i = 0; i < allProjects.Length; i++)
        {
            Project project = allProjects[i];
            ProjectVersion? latestProjectVersion =
                dbContext.ProjectVersions.FirstOrDefault(x => x.IsDefault && x.ProjectId == project.Id);
            if (latestProjectVersion == null)
                throw new ArgumentException($"Project {project.Name} is missing a default project version!");

            projectInfos[i] = new ProjectInfo
            {
                Name = project.Name,
                ShortName = project.ShortName,
                Description = project.Description,
                IconPath = project.IconPath,
                DefaultVersion = latestProjectVersion.VersionTag
            };
        }
        
        return View(projectInfos);
    }
    
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }
}