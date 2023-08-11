using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly BuildManager buildManager;

    public BuildService(ILogger<BuildService> logger, IDbContextFactory<VoltProjectDbContext> contextFactory, IOptions<VoltProjectsBuilderConfig> config, BuildManager buildManager)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
        this.config = config.Value;
        this.buildManager = buildManager;
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
                    .AsNoTracking()
                    .ToArrayAsync(cancellationToken: stoppingToken);
                
                logger.LogDebug("Found {ProjectCount} projects to build...", project.Length);
                await Parallel.ForEachAsync(project, stoppingToken, async (version, token) =>
                {
                    logger.LogInformation("Started building of project {Project}...", version.Project.Name);
                    
                    //Create a new DB Context
                    await using VoltProjectDbContext projectContext =
                        await contextFactory.CreateDbContextAsync(token);
                    
                    //Create a transaction, and a savepoint
                    IDbContextTransaction transaction = await projectContext.Database.BeginTransactionAsync(token);
                    await transaction.CreateSavepointAsync("BeforeUpdate", token);

                    try
                    {
                        buildManager.BuildProject(projectContext, version);
                        
                        //Add build message
                        projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
                        {
                            ProjectVersionId = version.Id,
                            Successful = true,
                            Date = DateTime.UtcNow,
                            Message = "Successfully Built Project",
                            GitHash = ""
                        });

                        await projectContext.SaveChangesAsync(token);
                        await transaction.CommitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        //We always want this to be written to the DB!
                        // ReSharper disable MethodHasAsyncOverloadWithCancellation
                        transaction.RollbackToSavepoint("BeforeUpdate");
                        logger.LogError(ex, "Error occured while building project {Project}!", version.Project.Name);
                        
                        //Add build error message
                        projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
                        {
                            ProjectVersionId = version.Id,
                            Successful = false,
                            Date = DateTime.UtcNow,
                            Message = $"Failed to build project! Error: {ex.Message}",
                            GitHash = ""
                        });

                        projectContext.SaveChanges();
                        transaction.Commit();
                        // ReSharper restore MethodHasAsyncOverloadWithCancellation
                        
                        return;
                    }
                    
                    logger.LogInformation("Successfully finished building project {Project}!", version.Project.Name);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Uncaught error while trying to build projects!");
            }
            
            await Task.Delay(config.DelayTime, stoppingToken);
        }
    }
}