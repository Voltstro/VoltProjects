using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_version")]
public class ProjectVersion
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }

    /// <summary>
    ///     Version tag.
    ///     <para>'latest' for latest</para>
    /// </summary>
    public string VersionTag { get; set; } = "latest";

    /// <summary>
    ///     What git branch does this version use?
    /// </summary>
    public string GitBranch { get; set; } = "master";
    
    /// <summary>
    ///     What git tag does this version use?
    /// </summary>
    public string? GitTag { get; set; }

    /// <summary>
    ///     What doc builder does this doc version use
    /// </summary>
    [ForeignKey("DocBuilder")]
    public string DocBuilderId { get; set; }
    public virtual DocBuilder DocBuilder { get; set; }

    /// <summary>
    ///     Where are the docs stored?
    /// </summary>
    public string DocsPath { get; set; } = "docs/";

    /// <summary>
    ///     Where are the docs built to?
    /// </summary>
    public string DocsBuiltPath { get; set; } = "docs/_site/";

    /// <summary>
    ///     What language does this doc use?
    /// </summary>
    [ForeignKey("Language")]
    public int LanguageId { get; set; } = 1;
    public virtual Language Language { get; set; }

    /// <summary>
    ///     Is this a default fallback doc version?
    /// </summary>
    public bool IsDefault { get; set; } = true;

    /// <summary>
    ///     Is this project version published
    /// </summary>
    public bool Published { get; set; } = true;

    /// <summary>
    ///     Is this project search engine indexable
    /// </summary>
    public bool SeIndexable { get; set; } = true;

    /// <summary>
    ///     When was the last time this project was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     When was this project created?
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     All <see cref="ProjectMenuItem"/>s this project has
    /// </summary>
    public ICollection<ProjectMenuItem>? MenuItems { get; } = new List<ProjectMenuItem>();
    
    /// <summary>
    ///     All <see cref="ProjectPage"/> this project has
    /// </summary>
    public ICollection<ProjectPage> Pages { get; } = new List<ProjectPage>();
    
    /// <summary>
    ///     All <see cref="ProjectPreBuild"/> this project has
    /// </summary>
    public ICollection<ProjectPreBuild> PreBuilds { get; set; } = new List<ProjectPreBuild>();
}