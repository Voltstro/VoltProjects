using System;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Server.Core.Git;

/// <summary>
///     Wrapper for interfacing with Git
/// </summary>
public sealed class Git
{
    private readonly ILogger<Git> _logger;

    public Git(ILogger<Git> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Pulls a repo (aka updating it) or cloning it if it doesn't exist at that path
    /// </summary>
    /// <param name="gitUrl"></param>
    /// <param name="gitBranch"></param>
    /// <param name="path"></param>
    public void PullRepoOrCloneIfDoesntExist(string gitUrl, string gitBranch, string path)
    {
        try
        {
            using Repository repo = new(path);
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
            CloneRepo(gitUrl, gitBranch, path);
        }
    }

    /// <summary>
    ///     Clones a repo
    /// </summary>
    /// <param name="gitUrl"></param>
    /// <param name="gitBranch"></param>
    /// <param name="path"></param>
    private void CloneRepo(string gitUrl, string gitBranch, string path)
    {
        Repository.Clone(gitUrl, path, new CloneOptions
        {
            BranchName = gitBranch,
            Checkout = true,
            OnProgress = OnProgressLog,
            OnTransferProgress = OnTransferLog,
            OnCheckoutProgress = OnCheckoutLog
        });
    }

    /// <summary>
    ///     Gets a repo's latest commit's hash
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetRepoCommitHash(string path)
    {
        using Repository repo = new(path);
        return repo.Head.Tip.Sha;
    }

    /// <summary>
    ///     Sets a repo to it's latest tag
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="TagException"></exception>
    public void SetToLatestTag(string path)
    {
        using Repository repo = new(path);
        if(!repo.Tags.Any())
            throw new TagException("The repo doesn't have any tags!");

        Tag? tag = repo.Tags.ElementAt(repo.Tags.Count() - 1);
        if (tag == null)
            throw new TagException("Fail to get tag!");

        Commands.Checkout(repo, tag.Target.Sha, new CheckoutOptions
        {
            OnCheckoutProgress = OnCheckoutLog
        });
    }

    #region Logging

    private bool OnProgressLog(string output)
    {
        _logger.LogDebug(output);
        return true;
    }

    private bool OnTransferLog(TransferProgress progress)
    {
        _logger.LogDebug("Git Progress: {ReceivedObjects}/{TotalObjects}, {Bytes} bytes", 
            progress.ReceivedObjects, progress.TotalObjects, progress.ReceivedBytes);
        return true;
    }

    private void OnCheckoutLog(string path, int completedSteps, int totalSteps)
    {
        _logger.LogDebug("{Path}: {Completed}/{Total}", path, completedSteps, totalSteps);
    }
    
    #endregion
}