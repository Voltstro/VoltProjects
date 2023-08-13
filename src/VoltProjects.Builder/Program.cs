using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder;
using VoltProjects.Builder.Data;
using VoltProjects.Shared;
using WebMarkupMin.Core;

//Create host
using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((context, collection) =>
    {
        //Setup Config
        collection.Configure<VoltProjectsBuilderConfig>(context.Configuration.GetSection("Config"));
        
        //Our singletons
        collection.AddSingleton<HtmlMinifier>();
        collection.AddSingleton<HtmlHighlightService>();
        collection.AddSingleton<ProjectRepoManager>();
        collection.AddSingleton<BuildManager>();
        
        //Background services
        collection.AddHostedService<BuildService>();
    
        //Setup DB
        collection.AddDbContextFactory<VoltProjectDbContext>(options =>
        {
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection"));
        });
    })
    .Build();

//Get logger factory and env
ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
IHostEnvironment hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

//Print some details
ILogger mainLogger = loggerFactory.CreateLogger(typeof(Program));
mainLogger.LogInformation("Starting VoltProject builder at {Time} running in {Mode} mode.", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"), hostEnvironment.EnvironmentName);

//Start
await host.RunAsync();

mainLogger.LogInformation("Shutting down at {Time}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));