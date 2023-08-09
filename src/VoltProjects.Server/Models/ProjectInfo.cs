namespace VoltProjects.Server.Models;

/// <summary>
///     Basic details on a project
/// </summary>
public struct ProjectInfo
{
    public string Name { get; init; }
    public string? ShortName { get; init; }
    public string Description { get; init; }
    public string DefaultVersion { get; init; }
    public string GitUrl { get; init; }
    
    public string IconPath { get; init; }
}