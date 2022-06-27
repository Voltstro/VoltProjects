using System;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Server.Core.Git;

/// <summary>
///     Wrapper for interfacing with Git
/// </summary>
public class Git
{
    private readonly ILogger<Git> _logger;

    public Git(ILogger<Git> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Clones a repo
    /// </summary>
    /// <param name="gitUrl"></param>
    /// <param name="gitBranch"></param>
    /// <param name="path"></param>
    public void CloneRepo(string gitUrl, string gitBranch, string path)
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