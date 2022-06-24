using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace VoltProjects.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseDefaultFiles();

            app.UseFileServer(new FileServerOptions
            {
                FileProvider =
                    new PhysicalFileProvider(
                        Path.Combine(AppContext.BaseDirectory, "Sites/VoltRpc")),
                RequestPath = "/VoltRpc",
                RedirectToAppendTrailingSlash = true,
                StaticFileOptions = { OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
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
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }}
            });
        }
    }
}