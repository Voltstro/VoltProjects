using System;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Models;

/// <summary>
///     Model for index page
/// </summary>
public class IndexPageModel
{
    public Project[] Projects { get; init; } = Array.Empty<Project>();
}