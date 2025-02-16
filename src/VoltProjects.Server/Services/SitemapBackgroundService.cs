using System;
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
using VoltProjects.Shared.Telemetry;

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
    
    private readonly string[] baseSitePaths = ["/", "/about/"];

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
            using (Tracking.StartActivity(ActivityArea.Sitemap, "generate"))
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
                indexSitemapRoot.Add(CreatePageLoc(xmlns, "sitemap.xml.gz", null, "sitemap"));
                
                //Add VoltProject's main pages
                VoltProjectDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);
                
                ProjectVersion[] projectVersions = await context.ProjectVersions
                    .Include(x => x.Project)
                    .Include(x => x.Pages.Where(y => y.Published))
                    .ToArrayAsync(stoppingToken);

                Dictionary<string, XDocument> projectSitemaps = new();
                foreach (ProjectVersion projectVersion in projectVersions)
                {
                    //Add Project's sitemap URL to root one
                    indexSitemapRoot.Add(CreatePageLoc(xmlns, Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag, "sitemap.xml.gz"), null, "sitemap"));

                    //And add all pages
                    XElement projectRoot = new(xmlns + "urlset");
                    foreach (ProjectPage page in projectVersion.Pages)
                    {
                        string pagePath = page.Path == "." ? string.Empty : $"{page.Path}";
                        string fullPath = $"{projectVersion.Project.Name}/{projectVersion.VersionTag}/{pagePath}";
                        projectRoot.Add(CreatePageLoc(xmlns, fullPath, page.PublishedDate));
                    }
                    
                    string projectKey = $"{projectVersion.Project.Name}/{projectVersion.VersionTag}";
                    projectSitemaps.Add(projectKey, new XDocument(projectRoot));
                }

                XDocument indexSitemap = new(indexSitemapRoot);
                XDocument baseSitemap = new(baseSitemapRoot);
                sitemapService.SetSitemaps(indexSitemap, baseSitemap, projectSitemaps);
                
                logger.LogInformation("Done generating new sitemap...");
            }

            //Wait for next generation time
            await Task.Delay(config.SitemapGenerationDelayTime, stoppingToken);
        }    
    }
    
    private XElement CreatePageLoc(XNamespace xNamespace, string url, DateTime? lastModTime = null, string element = "url")
    {
        string fullUrl = Path.Combine(config.SiteUrl, url);
        
        XElement urlElement = new(xNamespace + element);
        urlElement.Add(new XElement(xNamespace + "loc", fullUrl));
        if(lastModTime.HasValue)
            urlElement.Add(new XElement(xNamespace + "lastmod", lastModTime.Value.ToString("yyyy-MM-dd")));
        
        return urlElement;
    }
}