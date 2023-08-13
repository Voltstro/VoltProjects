using System.Diagnostics;
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
    private readonly ProjectRepoManager repoManager;
    private readonly BuildManager buildManager;

    public BuildService(ILogger<BuildService> logger, IDbContextFactory<VoltProjectDbContext> contextFactory, IOptions<VoltProjectsBuilderConfig> config, ProjectRepoManager repoManager, BuildManager buildManager)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
        this.config = config.Value;
        this.repoManager = repoManager;
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
                    logger.LogInformation("Started building of project {Project}...", project.Name);
                    
                    //First, get repo path
                    string repoPath = repoManager.GetProjectRepo(project);
                
                    //Create a new DB Context
                    await using VoltProjectDbContext projectContext =
                        await contextFactory.CreateDbContextAsync(token);

                    ProjectVersion[] projectVersions = await projectContext.ProjectVersions
                        .AsNoTracking()
                        .Where(x => x.ProjectId == project.Id)
                        .ToArrayAsync(token);

                    foreach (ProjectVersion projectVersion in projectVersions)
                    {
                        logger.LogInformation("Building project {ProjectName} version {Version}...", project.Name,
                            projectVersion.VersionTag);
                        
                        IDbContextTransaction transaction = await projectContext.Database.BeginTransactionAsync(token);
                        string transactionName = $"BeforeUpdate-{project.Name}";
                        await transaction.CreateSavepointAsync(transactionName, token);
                        
                        try
                        {
                            logger.LogDebug("Setting {Project} branch to {Branch}...", project.Name,
                                projectVersion.GitBranch);
                            repoManager.SetProjectRepoBranch(project, projectVersion.GitBranch);

                            //TODO: Check commits

                            //TODO: Execute pre-build commands
                            ProjectPreBuild[] prebuildCommands = await projectContext.PreBuildCommands
                                .AsNoTracking()
                                .Where(x => x.ProjectVersionId == projectVersion.Id)
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
                            buildManager.BuildProject(projectContext, projectVersion, repoPath);
                            
                            //Add build message
                            projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
                            {
                                ProjectVersionId = projectVersion.Id,
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
                            transaction.RollbackToSavepoint(transactionName);
                            logger.LogError(ex, "Error occured while building project {Project}!", project.Name);
                    
                            //Add build error message
                            projectContext.ProjectBuildEvents.Add(new ProjectBuildEvent
                            {
                                ProjectVersionId = projectVersion.Id,
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
                        
                        logger.LogInformation("Successfully finished building project {Project} version {Version}!", project.Name, projectVersion.VersionTag);
                    }
                    
                    logger.LogInformation("Finished building all versions in project {Project}", project.Name);
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