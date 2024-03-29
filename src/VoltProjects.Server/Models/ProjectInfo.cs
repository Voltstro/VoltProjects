namespace VoltProjects.Server.Models;

/// <summary>
///     Basic details on a project
/// </summary>
public class ProjectInfo
{
    /// <summary>
    ///     Name of the project
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Display name of the project
    /// </summary>
    public string DisplayName { get; init; }
    
    /// <summary>
    ///     Description of the project
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    ///     The URL to the project's git repo
    /// </summary>
    public string GitUrl { get; init; }
    
    /// <summary>
    ///     Path to the project's icon
    /// </summary>
    public string IconPath { get; init; }
    
    /// <summary>
    ///     Whats the projects default version
    /// </summary>
    public string DefaultVersion { get; init; }
    
    /// <summary>
    ///     All other versions
    /// </summary>
    public string[] OtherVersions { get; init; }
}