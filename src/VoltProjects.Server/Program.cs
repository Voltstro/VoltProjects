using System;
using System.Diagnostics;
using Figgle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;

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
    builder.Services.AddScoped<ProjectMenuService>();

    //Support razor pages runtime compilation for hot reloading
    IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews(
        mvcOptions => mvcOptions.Filters.Add<OperationCancelledExceptionFilter>());
    mvcBuilder.AddRazorRuntimeCompilation();

    //Allows for caching
    builder.Services.AddResponseCaching();
    
    //Db
    if (builder.Environment.IsDevelopment())
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    
    builder.Services.AddDbContextFactory<VoltProjectDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddDbContext<VoltProjectDbContext>(options => 
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    //Now setup the app
    WebApplication app = builder.Build();

    //Some configuration will change depending on the environment
    if (!app.Environment.IsDevelopment())
        app.UseHsts();
    else
        app.UseDeveloperExceptionPage();

    app.UseStaticFiles();
    
    string pattern = @"^(((.*/)|(/?))[^/.]+(?!/$))$";
    RewriteOptions options = new RewriteOptions()
        .AddRedirect(pattern, "$1/",301);
    app.UseRewriter(options);
    
    app.UseStatusCodePagesWithReExecute("/Eroor/{0}");
    app.UseResponseCaching();

    app.UseRouting();
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