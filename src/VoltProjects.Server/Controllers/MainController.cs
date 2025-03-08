using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Shared;
using VoltProjects.Server.Shared.Helpers;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
public class MainController : Controller
{
    private readonly IMemoryCache memoryCache;
    private readonly VoltProjectDbContext dbContext;
    private readonly VoltProjectsConfig config;
    
    public MainController(IMemoryCache memoryCache, VoltProjectDbContext dbContext, IOptions<VoltProjectsConfig> config)
    {
        this.memoryCache = memoryCache;
        this.dbContext = dbContext;
        this.config = config.Value;
    }

    [HttpGet]
    [Route("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ProjectInfo[] projectInfos = (await memoryCache.GetOrCreateAsync<ProjectInfo[]>("IndexProjects", async entry =>
        {
            List<ProjectInfo> projectInfos = new();

            ProjectVersion[] projects = await dbContext.ProjectVersions
                .AsNoTracking()
                .Include(x => x.Project)
                .OrderBy(x => x.Project.Name)
                .ToArrayAsync(cancellationToken);

            foreach (ProjectVersion project in projects)
            {
                ProjectInfo? projectInfo = projectInfos.FirstOrDefault(x => x.Name == project.Project.Name);
                if (projectInfo != null)
                    continue;
                
                projectInfo = new ProjectInfo
                {
                    Name = project.Project.Name,
                    DisplayName = project.Project.DisplayName,
                    Description = project.Project.Description,
                    IconPath = Path.Combine(config.PublicUrl, project.Project.Name, project.Project.IconPath!),
                    DefaultVersion =
                        projects.FirstOrDefault(x => x.IsDefault && x.Project.Name == project.Project.Name)!
                            .VersionTag,
                    OtherVersions = projects.Where(x => x.Project.Name == project.Project.Name && !x.IsDefault)
                        .Select(x => x.VersionTag).ToArray()
                };
                    
                projectInfos.Add(projectInfo);
            }
            
            //Expiry in 6 hours
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return projectInfos.ToArray();
        }))!;
        
        HttpContext.SetCacheControl(config.CacheTime);
        return View(projectInfos);
    }

    [Route("/about")]
    public IActionResult About()
    {
        AboutViewModel viewModel = new()
        {
            Developers = new AboutDev[]
            {
                new()
                {
                    Name = "Voltstro",
                    Description = "Initial work",
                    SocialLinks = new SocialLink[]
                    {
                        new()
                        {
                            Href = "https://github.com/Voltstro",
                            Icon = "github",
                            Name = "GitHub"
                        },
                        new()
                        {
                            Href = "https://x.com/Voltstro",
                            Icon = "twitter-x",
                            Name = "X (Formerly Twitter)"
                        },
                        new()
                        {
                            Href = "https://voltstro.dev",
                            Icon = "globe2",
                            Name = "Website"
                        }
                    }
                }
            },
            Projects = new OpenSourceProject[]
            {
                new()
                {
                    Name = ".NET",
                    Href = "https://dot.net",
                    LogoHref = "/assets/logo-dotnet.svg"
                },
                new()
                {
                    Name = "Bootstrap",
                    Href = "https://getbootstrap.com",
                    LogoHref = "/assets/logo-bootstrap.svg"
                },
                new()
                {
                    Name = "LibGit2Sharp",
                    Href = "https://github.com/libgit2/libgit2sharp",
                    LogoHref = "/assets/logo-libgit2sharp.svg"
                },
                new()
                {
                    Name = "Serilog",
                    Href = "https://serilog.net",
                    LogoHref = "/assets/logo-serilog.svg"
                },
                new()
                {
                    Name = "Vite",
                    Href = "https://vitejs.dev",
                    LogoHref = "/assets/logo-vite.svg"
                },
            }
        };
        
        return View(viewModel);
    }
}