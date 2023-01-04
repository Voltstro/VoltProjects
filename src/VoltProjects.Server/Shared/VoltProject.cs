using System.Diagnostics;
using static System.String;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace VoltProjects.Server.Shared;

/// <summary>
///     Details on a project to display on VoltProjects
/// </summary>
[DebuggerDisplay("Name = {Name}, Path = {GitPath}")]
public class VoltProject
{
    /// <summary>
    ///     The name of the project
    /// </summary>
    public string Name { get; init; } = Empty;

    /// <summary>
    ///     Short version of the name
    /// </summary>
    public string? NameShort { get; init; } = null;
    
    /// <summary>
    ///     Display this doc on the main index?
    /// </summary>
    public bool Hidden { get; init; }

    /// <summary>
    ///     The git 'path'
    /// </summary>
    public string GitPath { get; init; } = Empty;

    /// <summary>
    ///     What git branch to use
    /// </summary>
    public string GitBranch { get; init; } = "master";

    /// <summary>
    ///     Is this git repo a url
    /// </summary>
    public bool GitIsUrl => GitPath.StartsWith("https://");

    /// <summary>
    ///     What docs builder to use
    /// </summary>
    public string DocsBuilder { get; init; } = "DocFx";
    
    /// <summary>
    ///     Where are the actual docs located in the git repo
    /// </summary>
    public string DocsPath { get; init; } = "docs/";

    public string DocsOutputPath { get; init; } = "docs/_site/";

    /// <summary>
    ///     Path to the icon to use
    ///     <para>From the root of the repo</para>
    /// </summary>
    public string? IconPath { get; init; }
}
// ReSharper restore UnusedMember.Local
// ReSharper restore UnusedAutoPropertyAccessor.Local
// ReSharper restore UnusedAutoPropertyAccessor.Global