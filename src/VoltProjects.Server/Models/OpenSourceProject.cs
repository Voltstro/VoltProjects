namespace VoltProjects.Server.Models;

/// <summary>
///     An open-source project to display
/// </summary>
public struct OpenSourceProject
{
    /// <summary>
    ///     Name of the project
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Project's href
    /// </summary>
    public string Href { get; init; }
    
    /// <summary>
    ///     Href to the project's logo
    /// </summary>
    public string LogoHref { get; init; }
}