using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace VoltProjects.Server;

public static class SitesConfiguration
{
    public static IApplicationBuilder SetupVoltProjects(this IApplicationBuilder app, VoltProjectsConfig config)
    {
        app.UseFileServer(new FileServerOptions
        {
            FileProvider =
                new PhysicalFileProvider(
                    Path.Combine(AppContext.BaseDirectory, "Sites/VoltRpc")),
            RequestPath = "/VoltRpc",
            RedirectToAppendTrailingSlash = true,
            StaticFileOptions = { OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                    "public,max-age=" + config.HostCacheTime;
            }}
        });
            
        app.UseFileServer(new FileServerOptions
        {
            FileProvider =
                new PhysicalFileProvider(
                    Path.Combine(AppContext.BaseDirectory, "Sites/TestSite")),
            RequestPath = "/TestSite",
            RedirectToAppendTrailingSlash = true,
            StaticFileOptions = { OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                    "public,max-age=" + config.HostCacheTime;
            }}
        });

        return app;
    }
}