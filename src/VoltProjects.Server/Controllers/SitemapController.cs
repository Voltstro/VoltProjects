using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoltProjects.Server.Services;

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
    [Route("/sitemap_index.xml.gz")]
    public async Task<IActionResult> GetIndexSitemap(CancellationToken cancellationToken)
    {
        byte[] sitemap = await sitemapService.GetIndexSitemap(cancellationToken);
        return File(sitemap, "application/x-gzip");
    }
    
    [HttpGet]
    [Route("/sitemap.xml.gz")]
    public async Task<IActionResult> GetBaseSitemap(CancellationToken cancellationToken)
    {
        byte[] sitemap = await sitemapService.GetBaseSitemap(cancellationToken);
        return File(sitemap, "application/x-gzip");
    }

    [HttpGet]
    [Route("/{name}/{version}/sitemap.xml.gz")]
    public async Task<IActionResult> GetProjectSitemap(string name, string version, CancellationToken cancellationToken)
    {
        byte[]? sitemap = await sitemapService.GetProjectSitemap(name, version, cancellationToken);
        if (sitemap == null)
            return NotFound();
        
        return File(sitemap, "application/x-gzip");
    }
}