using System.Diagnostics;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Services;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

public sealed class BuildRunner : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly CancellationToken cancellationToken;
    
    private readonly IDbContextFactory<VoltProjectDbContext> dbContextFactory;
    private ProjectBuildSchedule buildSchedule;
    private readonly ILogger<BuildRunner> logger;
    private readonly ProjectRepoService repoService;
    private readonly BuildManager buildManager;
    
    public BuildRunner(
        IDbContextFactory<VoltProjectDbContext> dbContextFactory,
        ProjectBuildSchedule buildSchedule,
        ILogger<BuildRunner> logger,
        ProjectRepoService repoService,
        BuildManager buildManager)
    {
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        
        this.dbContextFactory = dbContextFactory;
        this.buildSchedule = buildSchedule;
        this.logger = logger;
        this.repoService = repoService;
        this.buildSchedule = buildSchedule;
        this.buildManager = buildManager;
    }

    public async Task Run()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DateTime nextExecuteTime;
            DateTime lastExecuteTime = buildSchedule.LastExecuteTime ?? DateTime.UtcNow;

            try
            {
                CronExpression expression = CronExpression.Parse(buildSchedule.Cron, CronFormat.IncludeSeconds);
                DateTime? decodedNextTIme = expression.GetNextOccurrence(lastExecuteTime);
                if(decodedNextTIme == null)
                    throw new NullReferenceException();

                nextExecuteTime = decodedNextTIme.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling cron decoding!");
                return;
            }
            
            TimeSpan delayTime = TimeSpan.Zero;
            if(nextExecuteTime > DateTime.UtcNow)
                delayTime = nextExecuteTime - lastExecuteTime;

            logger.LogInformation("Next build time for project {ProjectName} is at {NextExecuteTime}. Delaying for {DelayTime}", buildSchedule.ProjectVersion.Project.Name, nextExecuteTime, delayTime);
            try
            {
                await Task.Delay(delayTime, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Quitting build runner... was canceled.");
                return;
            }
            
            //Now we build project
            VoltProjectDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            try
            {
                ProjectVersion projectVersion = await dbContext.ProjectVersions
                    .Include(x => x.Project)
                    .Include(x => x.DocBuilder)
                    .Include(x => x.Language)
                    .FirstAsync(x => x.Id == buildSchedule.ProjectVersionId, cancellationToken);

                Project project = projectVersion.Project;
                
                string repoPath = repoService.GetProjectRepo(project);

                bool skipBuild = false;
                string projectCommitHash = repoService.GetProjectRepoGitHash(project);
                if (!projectVersion.Project.GitIsUrl)
                {
                    //Local repo
                    logger.LogWarning("{Project} is using local repo stored at {RepoPath}, not touching branches/tags/etc...", project.Name, project.GitUrl);
                }
                else
                {
                    //A repo we have on store
                    logger.LogDebug("Setting {Project} branch to {Branch}...", project.Name, projectVersion.GitBranch);
                    repoService.SetProjectRepoBranch(project, projectVersion.GitBranch);

                    //If last build event was a success, ignore it
                    if (!buildSchedule.IgnoreBuildEvents)
                    {
                        ProjectBuildEvent? lastBuildEvent = await dbContext.ProjectBuildEvents
                            .AsNoTracking()
                            .Where(x => x.ProjectVersionId == projectVersion.Id && x.BuilderVer == BuildService.BuildVer)
                            .OrderByDescending(x => x.CreationTime)
                            .FirstOrDefaultAsync(cancellationToken);

                        //Check commit hash is not the same
                        if (lastBuildEvent is { Successful: true })
                        {
                            //Hashes are the same, no need to rebuild
                            if (lastBuildEvent.GitHash == projectCommitHash)
                            {
                                logger.LogInformation("Project is already using latest build. Not re-building...");
                                skipBuild = true;
                            }
                        }
                    }
                }

                if (!skipBuild)
                {
                    //Create new build event
                    ProjectBuildEvent buildEvent = new()
                    {
                        ProjectVersionId = projectVersion.Id,
                        BuilderVer = BuildService.BuildVer,
                        Message = "Running Build...",
                        GitHash = projectCommitHash,
                        Successful = false
                    };
                    dbContext.ProjectBuildEvents.Add(buildEvent);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    
                    IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                    const string transactionName = "ProjectBuild";

                    //Run Build
                    await transaction.CreateSavepointAsync(transactionName, cancellationToken);
                    try
                    {
                        //Now run build
                        BuildProjectResult buildResult = await buildManager.BuildProject(dbContext, projectVersion, repoPath, cancellationToken);

                        //Add logs
                        foreach (ProjectBuildEventLog buildEventLog in buildResult.BuildEventLogs)
                        {
                            buildEventLog.BuildEventId = buildEvent.Id;
                        }
                        await dbContext.InsertProjectBuildEventLogs(buildResult.BuildEventLogs.ToArray());
                        
                        //Update event
                        buildEvent.Successful = true;
                        buildEvent.Message = "Successfully Built Project";
                    }
                    catch (Exception)
                    {
                        transaction.RollbackToSavepoint(transactionName);
                        throw;
                    }
                    finally
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Uncaught exception in build runner!");
            }
            finally
            {
                //Get tracked project schedule
                buildSchedule = dbContext.ProjectBuildSchedules
                    .Include(x => x.ProjectVersion)
                    .ThenInclude(x => x.Project)
                    .First(x => x.Id == buildSchedule.Id);
                buildSchedule.LastExecuteTime = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            
            //Now cleanup
            await dbContext.DisposeAsync();
        }
    }
    
    public void Dispose()
    {
        cancellationTokenSource.Cancel();
    }
}