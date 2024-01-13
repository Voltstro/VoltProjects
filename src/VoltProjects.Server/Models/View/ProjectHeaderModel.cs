using System;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

/// <summary>
///     Model for the project header
/// </summary>
public sealed class ProjectHeaderModel
{
    /// <summary>
    ///     Reference to the <see cref="ProjectPage"/>
    /// </summary>
    public ProjectPage ProjectPage { get; init; }
    
    /// <summary>
    ///    Full current URL to this page
    /// </summary>
    public Uri PageFullUrl { get; init; }
}