using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A project's TOC item
/// </summary>
[Table("project_toc")]
public class ProjectToc
{
    /// <summary>
    ///     <see cref="ProjectToc"/> primary key.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    ///     What project this TOC is for
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     Toc item's relativity in the project
    /// </summary>
    public string TocRel { get; set; }
    
    /// <summary>
    ///     When was the last time this TOC was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this TOC created?
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    public ICollection<ProjectTocItem> TocItems { get; } = new List<ProjectTocItem>();
}