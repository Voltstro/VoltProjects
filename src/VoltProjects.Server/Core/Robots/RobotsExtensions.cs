using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Server.Core.Robots;

public static class RobotsExtensions
{
    public static IServiceCollection AddSitemapService(this IServiceCollection services)
    {
        return services.AddSingleton<SitemapService>();
    }

    public static IApplicationBuilder UseSitemapMiddleware(this IApplicationBuilder builder)
    {
        return builder.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/sitemapindex.xml.gz") || ctx.Request.Path.StartsWithSegments("/sitemap.xml.gz"),
            app => app.UseMiddleware<SitemapMiddleware>());
    }
}