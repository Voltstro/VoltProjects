using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Server.Shared.Helpers;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     Main <see cref="Controller"/>, for the index and about pages
/// </summary>
public class MainController : Controller
{
    private readonly ProjectService projectService;
    private readonly VoltProjectsConfig config;
    
    public MainController(ProjectService projectService, IOptions<VoltProjectsConfig> config)
    {
        this.projectService = projectService;
        this.config = config.Value;
    }

    [HttpGet]
    [Route("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Project[] projects = await projectService.GetProjects();
        
        HttpContext.SetCacheControl(config.CacheTime);
        return View(new MainViewModel(projects, config.PublicUrl));
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