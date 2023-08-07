using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("ProjectVersion")]
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
    [Required]
    public string VersionTag { get; set; }

    /// <summary>
    ///     What doc builder does this doc version use
    /// </summary>
    [ForeignKey("DocBuilder")]
    public string DocBuilderId { get; set; }
    public virtual DocBuilder DocBuilder { get; set; }

    /// <summary>
    ///     Where are the docs stored?
    /// </summary>
    [Required]
    public string DocsPath { get; set; }
    
    /// <summary>
    ///     Where are the docs built to?
    /// </summary>
    [Required]
    public string DocsBuiltPath { get; set; }
    
    /// <summary>
    ///     What language does this doc use?
    /// </summary>
    [ForeignKey("DocsLang")]
    public int DocsLangId { get; set; }
    public virtual Language DocsLang { get; set; }
    
    /// <summary>
    ///     Is this a default fallback doc version?
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    ///     Pre-build commands that this project version should do
    /// </summary>
    public virtual ICollection<PreBuildCommand> PreBuildCommands { get; set; } = new List<PreBuildCommand>();

    /// <summary>
    ///     Project menu items
    /// </summary>
    public virtual ICollection<ProjectMenu> ProjectsMenus { get; set; } = new List<ProjectMenu>();
}