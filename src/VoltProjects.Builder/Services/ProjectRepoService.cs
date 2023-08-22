using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Services;

/// <summary>
///     Handles a <see cref="Project"/>'s git repo
/// </summary>
public sealed class ProjectRepoService
{
    private readonly ILogger<ProjectRepoService> logger;
    private readonly VoltProjectsBuilderConfig config;
    
    /// <summary>
    ///     Creates a new <see cref="ProjectRepoService"/>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public ProjectRepoService(ILogger<ProjectRepoService> logger, IOptions<VoltProjectsBuilderConfig> config)
    {
        this.logger = logger;
        this.config = config.Value;
    }
    
    /// <summary>
    ///     Gets a project repo's path
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public string GetProjectRepo(Project project)
    {
        string projectRepoPath = Path.GetFullPath(Path.Combine(config.RepoStoreLocation, project.Name));
        
        try
        {
            using Repository repo = new(projectRepoPath);
            repo.RemoveUntrackedFiles();
            
            //Pull
            Commands.Pull(repo, new Signature("VoltProjects", "VoltProjects", DateTimeOffset.Now), new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    OnProgress = OnProgressLog,
                    OnTransferProgress = OnTransferLog
                }
            });
        }
        //If the repo wasn't found, then clone it
        catch (RepositoryNotFoundException)
        {
            logger.LogWarning("Cloning repo for {ProjectName} as it doesn't exist on disk...", project.Name);
            CloneRepo(project.GitUrl, projectRepoPath);
        }

        return projectRepoPath;
    }

    /// <summary>
    ///     Set's a project's repo branch
    /// </summary>
    /// <param name="project"></param>
    /// <param name="branch"></param>
    public void SetProjectRepoBranch(Project project, string branch)
    {
        string projectRepoPath = Path.GetFullPath(Path.Combine(config.RepoStoreLocation, project.Name));
        using Repository repo = new(projectRepoPath);

        Commands.Checkout(repo, branch);
    }
    
    /// <summary>
    ///     Clones a repo
    /// </summary>
    /// <param name="gitUrl"></param>
    /// <param name="path"></param>
    private void CloneRepo(string gitUrl, string path)
    {
        Repository.Clone(gitUrl, path, new CloneOptions
        {
            Checkout = true,
            OnProgress = OnProgressLog,
            OnTransferProgress = OnTransferLog,
            OnCheckoutProgress = OnCheckoutLog
        });
    }
    
    #region Logging

    private bool OnProgressLog(string output)
    {
        logger.LogDebug(output);
        return true;
    }

    private bool OnTransferLog(TransferProgress progress)
    {
        logger.LogDebug("Git Progress: {ReceivedObjects}/{TotalObjects}, {Bytes} bytes", 
            progress.ReceivedObjects, progress.TotalObjects, progress.ReceivedBytes);
        return true;
    }

    private void OnCheckoutLog(string path, int completedSteps, int totalSteps)
    {
        logger.LogDebug("{Path}: {Completed}/{Total}", path, completedSteps, totalSteps);
    }
    
    #endregion
}