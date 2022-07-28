using System;
using System.Diagnostics;
using static System.String;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace VoltProjects.Server.Core.SiteCache.Config;

/// <summary>
///     Details on a project to display on VoltProjects
/// </summary>
[DebuggerDisplay("Name = {Name}, Path = {_gitPath}, Remote = {GitIsRemote}")]
public class VoltProject
{
    /// <summary>
    ///     The name of the project
    /// </summary>
    public string Name { get; init; } = Empty;

    private readonly string _gitPath = Empty;
    
    /// <summary>
    ///     The git 'path'
    ///     <para>Path could be a https:// URL, or a local path to a git repo on this system</para>
    /// </summary>
    public string GitPath
    {
        get => _gitPath;
        init
        {
            _gitPath = value;
            if (value.StartsWith("https://"))
                GitIsRemote = true;
        }
    }

    /// <summary>
    ///     Is this a remote git repo (aka not local)
    /// </summary>
    public bool GitIsRemote { get; private init; }
    
    /// <summary>
    ///     The branch to use if remote
    /// </summary>
    public string GitBranch { get; init; } = Empty;

    /// <summary>
    ///     Use the latest tag instead of the latest commit on remote cloned repos?
    /// </summary>
    public bool GitUseLatestTag { get; init; }

    /// <summary>
    ///     Path to the icon to use
    ///     <para>From the root of the repo</para>
    /// </summary>
    public string? IconPath { get; init; }
    
    /// <summary>
    ///     Where are docs located in the repo
    ///     <para>From the root of the repo</para>
    /// </summary>
    public string DocsPath { get; init; } = Empty;
    
    /// <summary>
    ///     Where are docs built to
    ///     <para>From <see cref="DocsPath"/></para>
    /// </summary>
    public string DocsBuildDist { get; init; } = Empty;
    
    //NOTE: These settings should not be set in appsettings!!!
    #region Non-Settings

    internal bool HasSitemap { get; set; }

    #endregion
}
// ReSharper restore UnusedMember.Local
// ReSharper restore UnusedAutoPropertyAccessor.Local
// ReSharper restore UnusedAutoPropertyAccessor.Global