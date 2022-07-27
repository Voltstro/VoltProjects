using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VoltProjects.Server.Core.Robots;

public class SitemapMiddleware
{
    private readonly SitemapService _sitemapService;
    private readonly RequestDelegate _next;
    
    public SitemapMiddleware(SitemapService sitemapService, RequestDelegate next)
    {
        _sitemapService = sitemapService;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/sitemapindex.xml.gz"))
        {
            //TODO
        }
        else if (context.Request.Path.StartsWithSegments("/sitemap.xml.gz"))
        {
            context.Response.ContentType = "application/x-gzip";
            await context.Response.BodyWriter.WriteAsync(_sitemapService.CompressedBaseSitemap);
        }
        else
        {
            await _next(context);
        }
    }
}