using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Server.Services.Robots;

public static class RobotsExtensions
{
    public static IServiceCollection AddSitemapService(this IServiceCollection services)
    {
        return services.AddSingleton<SitemapService>();
    }

    public static IApplicationBuilder UseSitemapMiddleware(this IApplicationBuilder builder)
    {
        return builder.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/sitemap_index.xml.gz") || ctx.Request.Path.StartsWithSegments("/sitemap.xml.gz"),
            app => app.UseMiddleware<SitemapMiddleware>());
    }
}