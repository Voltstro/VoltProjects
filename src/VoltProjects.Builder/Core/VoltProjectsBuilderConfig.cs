namespace VoltProjects.Builder.Core;

/// <summary>
///     Configuration for VoltProjects.Builder to use
/// </summary>
public sealed class VoltProjectsBuilderConfig
{
    /// <summary>
    ///     Delay time before attempting to do a rebuild
    /// </summary>
    public TimeSpan DelayTime { get; init; }

    /// <summary>
    ///     Where to store project git repos?
    /// </summary>
    public string RepoStoreLocation { get; init; } = "Data/Repos/";
}