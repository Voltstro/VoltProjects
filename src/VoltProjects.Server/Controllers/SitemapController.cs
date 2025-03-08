using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Services;
using VoltProjects.Shared.Services;

namespace VoltProjects.Server.Controllers;

/// <summary>
///     <see cref="Controller"/> for sitemap files
/// </summary>
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class SitemapController : Controller
{
    private readonly SitemapService sitemapService;
    
    public SitemapController(SitemapService sitemapService)
    {
        this.sitemapService = sitemapService;
    }
    
    [HttpGet]
    [Route("/sitemap_index.xml")]
    public async Task<IActionResult> GetIndexSitemap(CancellationToken cancellationToken)
    {
        string siteIndexSitemap = await sitemapService.GetSiteIndexSitemap();
        return new ContentResult
        {
            Content = siteIndexSitemap,
            ContentType = "application/xml",
            StatusCode = 200
        };
    }
    
    [HttpGet]
    [Route("/sitemap.xml")]
    public async Task<IActionResult> GetBaseSitemap(CancellationToken cancellationToken)
    {
        string siteSitemap = await sitemapService.GetSiteSitemap();
        return new ContentResult
        {
            Content = siteSitemap,
            ContentType = "application/xml",
            StatusCode = 200
        };
    }

    [HttpGet]
    [Route("/{name}/{version}/sitemap.xml")]
    public async Task<IActionResult> GetProjectSitemap(string name, string version, CancellationToken cancellationToken)
    {
        string? sitemapDocument = await sitemapService.GetProjectSitemap(name, version);
        if(sitemapDocument == null)
            return NotFound();
        
        return new ContentResult
        {
            Content = sitemapDocument,
            ContentType = "application/xml",
            StatusCode = 200
        };
    }
}