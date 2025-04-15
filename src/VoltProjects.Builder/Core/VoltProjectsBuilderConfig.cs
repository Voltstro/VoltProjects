using VoltProjects.Shared.Services.Storage;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Configuration for VoltProjects.Builder to use
/// </summary>
public sealed class VoltProjectsBuilderConfig
{
    /// <summary>
    ///     Delay time for each job schedule check
    /// </summary>
    public TimeSpan DelayTime { get; init; }

    /// <summary>
    ///     Where to store project git repos?
    /// </summary>
    public string RepoStoreLocation { get; init; } = "Repos/";
    
    /// <summary>
    ///     Configuration for Cloud storage
    /// </summary>
    public StorageConfig? ObjectStorageProvider { get; init; }
}