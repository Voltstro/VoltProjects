using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

public sealed class ProjectViewModel
{
    /// <summary>
    ///     Base path of the project
    /// </summary>
    public string BasePath { get; init; }
    
    /// <summary>
    ///     Main details on the project page
    /// </summary>
    public ProjectPage ProjectPage { get; init; }
    
    /// <summary>
    ///     Model for the project nav
    /// </summary>
    public ProjectNavModel ProjectNavModel { get; init; }
    
    /// <summary>
    ///     Project's TOC
    /// </summary>
    public TocItem? Toc { get; init; }
}