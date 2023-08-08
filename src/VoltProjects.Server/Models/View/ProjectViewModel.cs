using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

public sealed class ProjectViewModel
{
    /// <summary>
    ///     Main details on the project page
    /// </summary>
    public ProjectPage ProjectPage { get; init; }
    
    /// <summary>
    ///     Main project menu
    /// </summary>
    public ProjectMenu ProjectMenu { get; init; }
    
    /// <summary>
    ///     Project's TOC
    /// </summary>
    public TocItem? Toc { get; init; }
}