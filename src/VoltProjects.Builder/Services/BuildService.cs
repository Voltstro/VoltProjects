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
    private const int BuildVer = 1;
    
    private readonly ILogger<BuildService> logger;
    private readonly IDbContextFactory<VoltProjectDbContext> contextFactory;
    private readonly VoltProjectsBuilderConfig config;
    private readonly ProjectRepoService repoService;
    private readonly BuildManager buildManager;

    public BuildService(
        ILogger<BuildService> logger,
        IDbContextFactory<VoltProjectDbContext> contextFactory,
        IOptions<VoltProjectsBuilderConfig> config,
        ProjectRepoService repoService,
        BuildManager buildManager)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
        this.config = config.Value;
        this.repoService = repoService;
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
                
                //First, get all projects
                Project[] projects = await context.Projects.AsNoTracking().ToArrayAsync(stoppingToken);
                logger.LogDebug("Found {ProjectCount} projects to build...", projects.Length);
                
                //Build each project
                await Parallel.ForEachAsync(projects, stoppingToken, async (project, token) =>
                {
                    await BuildProject(project, token);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Uncaught error while trying to build projects!");
            }
            
            await Task.Delay(config.DelayTime, stoppingToken);
        }
    }

    /// <summary>
    ///     Builds a specific <see cref="Project"/>
    /// </summary>
    /// <param name="project"></param>
    /// <param name="token"></param>
    public async Task BuildProject(Project project, CancellationToken token)
    {
        logger.LogInformation("Started building of project {Project}...", project.Name);
                    
        //First, get repo path
        string repoPath = repoService.GetProjectRepo(project);
                
        //Create a new DB Context
        await using VoltProjectDbContext projectContext =
            await contextFactory.CreateDbContextAsync(token);

        ProjectVersion[] projectVersions = await projectContext.ProjectVersions
            .AsNoTracking()
            .Include(x => x.DocBuilder)
            .Where(x => x.ProjectId == project.Id)
            .ToArrayAsync(token);

        foreach (ProjectVersion projectVersion in projectVersions)
        {
            projectVersion.Project = project;
            await BuildProjectVersion(projectVersion, projectContext, repoPath, token);
        }
                    
        logger.LogInformation("Finished building all versions in project {Project}", project.Name);
    }

    /// <summary>
    ///     Builds a specific <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="projectVersion"></param>
    /// <param name="projectContext"></param>
    /// <param name="repoPath"></param>
    /// <param name="token"></param>
    /// <exception cref="Exception"></exception>
    public async Task BuildProjectVersion(ProjectVersion projectVersion, VoltProjectDbContext projectContext, string repoPath, CancellationToken token)
    {
        Project project = projectVersion.Project;
        logger.LogInformation("Building project {ProjectName} version {Version}...", project.Name,
                            projectVersion.VersionTag);
                        
        IDbContextTransaction transaction = await projectContext.Database.BeginTransactionAsync(token);
        string transactionName = $"BeforeUpdate-{project.Name}";
        await transaction.CreateSavepointAsync(transactionName, token);

        string projectCommitHash = string.Empty;
        try
        {
            if (!project.GitIsUrl)
            {
                //Local repo
                logger.LogWarning("{Project} is using local repo stored at {RepoPath}, not touching branches/tags/etc...", project.Name, project.GitUrl);
            }
            else
            {
                //A repo we have on store
                logger.LogDebug("Setting {Project} branch to {Branch}...", project.Name, projectVersion.GitBranch);
                repoService.SetProjectRepoBranch(project, projectVersion.GitBranch);
                projectCommitHash = repoService.GetProjectRepoGitHash(project);

                ProjectBuildEvent? lastBuildEvent = await projectContext.ProjectBuildEvents
                    .AsNoTracking()
                    .Where(x => x.ProjectVersionId == projectVersion.Id && x.BuilderVer == BuildVer)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefaultAsync(token);

                //Check commit hash is not the same
                if (lastBuildEvent is { Successful: true })
                {
                    //Hashes are the same, no need to rebuild
                    if (lastBuildEvent.GitHash == projectCommitHash)
                    {
                        logger.LogInformation("Project is already using latest build. Not re-building...");
                        return;
                    }
                }
            }
           

            ProjectPreBuild[] prebuildCommands = await projectContext.PreBuildCommands
                .AsNoTracking()
                .Where(x => x.ProjectVersionId == projectVersion.Id)
                .OrderBy(x => x.Order)
                .ToArrayAsync(token);
            foreach (ProjectPreBuild prebuildCommand in prebuildCommands)
            {
                ProcessStartInfo startInfo = new(prebuildCommand.Command, prebuildCommand.Arguments)
                {
                    WorkingDirectory = repoPath
                };

                Process actionProcess = new()
                {
                    StartInfo = startInfo
                };

                actionProcess.Start();
                actionProcess.WaitForExit();

                if (actionProcess.ExitCode != 0)
                    throw new Exception("Action process failed to run!");

                actionProcess.Kill(true);
                actionProcess.Dispose();
            }

            //Build the project, and add deds to DB
            await buildManager.BuildProject(projectContext, projectVersion, repoPath, token);

            //Add build message
            projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
            {
                ProjectVersionId = projectVersion.Id,
                Successful = true,
                Message = "Successfully Built Project",
                GitHash = projectCommitHash,
                BuilderVer = BuildVer
            });
        }
        catch (Exception ex)
        {
            //We always want this to be written to the DB!
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            transaction.RollbackToSavepoint(transactionName);
            logger.LogError(ex, "Error occured while building project {Project}!", project.Name);

            //Add build error message
            projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
            {
                ProjectVersionId = projectVersion.Id,
                Successful = false,
                Message = $"Failed to build project! Error: {ex.Message}",
                GitHash = projectCommitHash,
                BuilderVer = BuildVer
            });

            return;
        }
        finally
        {
            //We always want this to be written to the DB!
            // ReSharper disable MethodHasAsyncOverloadWithCancellation
            projectContext.SaveChanges();
            transaction.Commit();
            // ReSharper restore MethodHasAsyncOverloadWithCancellation
        }
        
        logger.LogInformation("Successfully finished building project {Project} version {Version}!", project.Name, projectVersion.VersionTag);
    }
}