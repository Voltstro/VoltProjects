using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Should contain a Project's built menu, TOCs and pages
/// </summary>
public struct BuildResult
{
    /// <summary>
    ///     Creates a new <see cref="BuildResult"/> instance
    /// </summary>
    /// <param name="projectMenu"></param>
    /// <param name="tocs"></param>
    /// <param name="pages"></param>
    public BuildResult(ProjectMenu projectMenu, ProjectToc[] tocs, ProjectPage[] pages)
    {
        ProjectMenu = projectMenu;
        ProjectTocs = tocs;
        ProjectPages = pages;
    }
    
    public ProjectMenu ProjectMenu { get; }
    
    public ProjectToc[] ProjectTocs { get; }
    
    public ProjectPage[] ProjectPages { get; }
}