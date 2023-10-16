using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Shared;
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
        ProjectInfo[] projectInfos = await memoryCache.GetOrCreateAsync<ProjectInfo[]>("IndexProjects", async entry =>
        {
            Project[] allProjects = await dbContext.Projects.ToArrayAsync(cancellationToken);

            ProjectInfo[] projectInfos = new ProjectInfo[allProjects.Length];
            for (int i = 0; i < allProjects.Length; i++)
            {
                Project project = allProjects[i];

                //Get all versions
                ProjectVersion[] allProjectVersions =
                    await dbContext.ProjectVersions
                        .AsNoTracking()
                        .Where(x => x.ProjectId == project.Id)
                        .ToArrayAsync(cancellationToken);

                ProjectVersion? latestProjectVersion = allProjectVersions.FirstOrDefault(x => x.IsDefault);
                if (latestProjectVersion == null)
                    throw new ArgumentException($"Project {project.Name} is missing a default project version!");

                //Get all other versions
                string[] allOtherVersions = allProjectVersions.Where(x => !x.IsDefault)
                    .Select(projectVersion => projectVersion.VersionTag)
                    .ToArray();

                projectInfos[i] = new ProjectInfo
                {
                    Name = project.Name,
                    Description = project.Description,
                    IconPath = Path.Combine(config.PublicUrl, project.Name, project.IconPath),
                    DefaultVersion = latestProjectVersion.VersionTag,
                    OtherVersions = allOtherVersions
                };
            }

            //Expiry in 6 hours
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return projectInfos;
        });
        
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
                            Href = "https://twitter.com/Voltstro",
                            Icon = "twitter",
                            Name = "Twitter"
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