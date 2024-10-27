using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Services;

public sealed class BuildService : BackgroundService
{
    public const int BuildVer = 3;
    
    private readonly ILogger<BuildService> logger;
    private readonly ILoggerFactory loggerFactory;
    private readonly IDbContextFactory<VoltProjectDbContext> contextFactory;
    private readonly VoltProjectsBuilderConfig config;
    private readonly ProjectRepoService repoService;
    private readonly BuildManager buildManager;

    private Dictionary<ProjectBuildSchedule, BuildRunner> buildRunners;

    public BuildService(
        ILogger<BuildService> logger,
        ILoggerFactory loggerFactory,
        IDbContextFactory<VoltProjectDbContext> contextFactory,
        IOptions<VoltProjectsBuilderConfig> config,
        ProjectRepoService repoService,
        BuildManager buildManager)
    {
        this.logger = logger;
        this.loggerFactory = loggerFactory;
        this.contextFactory = contextFactory;
        this.config = config.Value;
        this.repoService = repoService;
        this.buildManager = buildManager;
        buildRunners = new Dictionary<ProjectBuildSchedule, BuildRunner>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting VoltProjects Builder service...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await using VoltProjectDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);

            ProjectBuildSchedule[] buildSchedules = await context.ProjectBuildSchedules
                .AsNoTracking()
                .Include(x => x.ProjectVersion)
                .ThenInclude(x => x.Project)
                .Where(x => x.IsActive)
                .ToArrayAsync(stoppingToken);
            
            logger.LogInformation("Checking build schedules with active build runners...");
            
            //Remove any old runners that are no-longer active
            foreach (KeyValuePair<ProjectBuildSchedule,BuildRunner> buildRunner in buildRunners)
            {
                ProjectBuildSchedule? buildSchedule = buildSchedules.FirstOrDefault(x => x.Id == buildRunner.Key.Id);
                if(buildSchedule != null)
                    continue;
                
                buildRunner.Value.Dispose();
                buildRunners.Remove(buildRunner.Key);
                logger.LogInformation("Removed build schedule {BuildSchedule} runner as it appears to no-longer be active.", buildRunner.Key.Id);
            }

            //Create any new build runners that do not exist already
            foreach (ProjectBuildSchedule buildSchedule in buildSchedules)
            {
                KeyValuePair<ProjectBuildSchedule, BuildRunner> runner = buildRunners.FirstOrDefault(x => x.Key.Id == buildSchedule.Id);
                if (runner.Value != null)
                    continue;
                
                //Create new logger and runner
                ILogger<BuildRunner> buildRunnerLogger = loggerFactory.CreateLogger<BuildRunner>();
                BuildRunner buildRunner = new(contextFactory, buildSchedule, buildRunnerLogger, repoService, buildManager);
                    
                logger.LogInformation("Created new build runner for project {ProjectName}, schedule {ScheduleId}", buildSchedule.ProjectVersion.Project.Name, buildSchedule.Id);
                _ = Task.Run(() => buildRunner.Run(), stoppingToken);
                buildRunners.Add(buildSchedule, buildRunner);
            }

            await Task.Delay(config.DelayTime, stoppingToken);
        }
    }
}