using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Data;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder;

internal sealed class BuildService : BackgroundService
{
    private readonly ILogger<BuildService> logger;
    private readonly IDbContextFactory<VoltProjectDbContext> contextFactory;
    private readonly VoltProjectsBuilderConfig config;
    private readonly Dictionary<string, Builder> builders;

    public BuildService(ILogger<BuildService> logger, IDbContextFactory<VoltProjectDbContext> contextFactory, IOptions<VoltProjectsBuilderConfig> configOptions, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
        config = configOptions.Value;
        
        builders = new Dictionary<string, Builder>();
        IEnumerable<Type> foundBuilders = ReflectionHelper.GetInheritedTypes<Builder>();

        foreach (Type foundBuilder in foundBuilders)
        {
            BuilderNameAttribute attribute = (BuilderNameAttribute)Attribute.GetCustomAttribute(foundBuilder, typeof(BuilderNameAttribute))!;

            Builder builder = (Builder)ActivatorUtilities.CreateInstance(serviceProvider, foundBuilder);
            builders.Add(attribute.Name, builder);
            this.logger.LogDebug("Created builder {Builder}", attribute.Name);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting VoltProjects Builder service...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using VoltProjectDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);
                ProjectVersion[] project = await context.ProjectVersions
                    .Include(x => x.Project)
                    .Include(x => x.DocBuilder)
                    .ToArrayAsync(cancellationToken: stoppingToken);
                
                logger.LogDebug("Found {ProjectCount} projects to build...", project.Length);
                foreach (ProjectVersion projectVersion in project)
                {
                    try
                    {
                        //Get project builder
                        KeyValuePair<string, Builder>? builder = builders.FirstOrDefault(x => x.Key == projectVersion.DocBuilder.Name);
                        if (builder == null)
                            throw new Exception($"Builder {projectVersion.DocBuilder.Name} doesn't exist!");
                        
                        builder.Value.Value.BuildProject(projectVersion, context);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error building project {Project}!", projectVersion.Project.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Uncaught error while trying to build projects!");
            }
            
            await Task.Delay(config.DelayTime, stoppingToken);
        }
    }
}