using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Services;

/// <summary>
///     <see cref="BackgroundService"/> for generating sitemaps
/// </summary>
public sealed class SitemapBackgroundService : BackgroundService
{
    private readonly ILogger<SitemapBackgroundService> logger;
    private readonly VoltProjectsConfig config;
    private readonly IDbContextFactory<VoltProjectDbContext> contextFactory;
    private readonly SitemapService sitemapService;
    
    private readonly string[] baseSitePaths = { "/", "/about/" };

    public SitemapBackgroundService(
        ILogger<SitemapBackgroundService> logger,
        IOptions<VoltProjectsConfig> config,
        IDbContextFactory<VoltProjectDbContext> contextFactory,
        SitemapService sitemapService)
    {
        this.logger = logger;
        this.config = config.Value;
        this.contextFactory = contextFactory;
        this.sitemapService = sitemapService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Generating new sitemap...");
            
            //Base XML document
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement baseSitemapRoot = new(xmlns + "urlset");

            //Add VoltProject's pages
            foreach (string baseSitePath in baseSitePaths)
            {
                baseSitemapRoot.Add(CreatePageLoc(xmlns, baseSitePath));
            }
            
            //Setup index sitemap
            XElement indexSitemapRoot = new(xmlns + "sitemapindex");
            indexSitemapRoot.Add(CreatePageLoc(xmlns, "sitemap.xml.gz", "sitemap"));
            
            //Add VoltProject's main pages
            VoltProjectDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);
            
            //Get all projects, and go through their project versions
            Dictionary<string, XDocument> projectSitemaps = new();
            Project[] projects = await context.Projects
                .AsNoTracking()
                .ToArrayAsync(stoppingToken);
            foreach (Project project in projects)
            {
                ProjectVersion[] versions = await context.ProjectVersions
                    .AsNoTracking()
                    .Where(x => x.ProjectId == project.Id).ToArrayAsync(stoppingToken);

                foreach (ProjectVersion version in versions)
                {
                    //Add Project's sitemap URL to root one
                    indexSitemapRoot.Add(CreatePageLoc(xmlns, Path.Combine(project.Name, version.VersionTag, "sitemap.xml.gz"), "sitemap"));
                    
                    //And generate project's sitemap
                    XElement projectRoot = new(xmlns + "urlset");
                    ProjectPage[] pages = await context.ProjectPages
                        .AsNoTracking()
                        .Where(x => x.ProjectVersionId == version.Id)
                        .ToArrayAsync(stoppingToken);
                    foreach (ProjectPage page in pages)
                    {
                        projectRoot.Add(CreatePageLoc(xmlns, Path.Combine(project.Name, version.VersionTag, page.Path)));
                    }

                    string projectKey = $"{project.Name}/{version.VersionTag}";
                    projectSitemaps.Add(projectKey, new XDocument(projectRoot));
                }
            }

            XDocument indexSitemap = new(indexSitemapRoot);
            XDocument baseSitemap = new(baseSitemapRoot);
            sitemapService.SetSitemaps(indexSitemap, baseSitemap, projectSitemaps);
            
            logger.LogInformation("Done generating new sitemap...");

            //Wait for next generation time
            await Task.Delay(config.SitemapGenerationDelayTime, stoppingToken);
        }    
    }
    
    private XElement CreatePageLoc(XNamespace xNamespace, string url, string element = "url")
    {
        string fullUrl = Path.Combine(config.SiteUrl, url);
        
        XElement urlElement = new(
            xNamespace + element,
            new XElement(xNamespace + "loc", fullUrl));
        return urlElement;
    }
}