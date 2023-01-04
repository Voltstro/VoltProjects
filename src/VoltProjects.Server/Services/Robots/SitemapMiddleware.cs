using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VoltProjects.Server.Services.Robots;

public class SitemapMiddleware
{
    private readonly SitemapService sitemapService;
    private readonly RequestDelegate next;
    
    public SitemapMiddleware(SitemapService sitemapService, RequestDelegate next)
    {
        this.sitemapService = sitemapService;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/sitemap_index.xml.gz"))
        {
            context.Response.ContentType = "application/x-gzip";
            await context.Response.BodyWriter.WriteAsync(sitemapService.CompressedIndexSitemap);
        }
        else if (context.Request.Path.StartsWithSegments("/sitemap.xml.gz"))
        {
            context.Response.ContentType = "application/x-gzip";
            await context.Response.BodyWriter.WriteAsync(sitemapService.CompressedBaseSitemap);
        }
        else
        {
            await next(context);
        }
    }
}