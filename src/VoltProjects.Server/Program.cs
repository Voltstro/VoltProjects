using System;
using System.Diagnostics;
using Figgle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.Server.Core.Robots;
using VoltProjects.Server.Shared;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

//Setup logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

//NOTE: We are adding a new line due to all the stuff at the start of each log message
Log.Information($"\n{FiggleFonts.Graffiti.Render("VoltProjects")}");
Log.Information("VoltProjects starting...");

try
{
    //Setup host
    builder.Host.UseSerilog();
    builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

    //Setup services
    builder.Services.Configure<VoltProjectsConfig>(
        builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects));
    builder.Services.AddSitemapService();

    //Support razor pages runtime compilation for hot reloading
    IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews();
#if DEBUG
    if (builder.Environment.IsDevelopment())
        mvcBuilder.AddRazorRuntimeCompilation();
#endif

    //Allows for caching
    builder.Services.AddResponseCaching();

    //Now setup the app
    WebApplication app = builder.Build();
    app.UseStaticFiles();
    app.UseStatusCodePagesWithReExecute("/Eroor/{0}");
    app.UseSitemapMiddleware();
    app.UseResponseCaching();
    app.UseRouting();

    //Some configuration will change depending on the environment
    if (!app.Environment.IsDevelopment())
        app.UseHsts();
    else
        app.UseDeveloperExceptionPage();

    //Setup our views/controllers
    app.MapControllers();

    Log.Information("Configuration done! Starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "An uncaught error occured!");
#if DEBUG
    if (Debugger.IsAttached)
        throw;
#endif
    
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;