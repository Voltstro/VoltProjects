using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_menu")]
public class ProjectMenu
{
    /// <summary>
    ///     <see cref="ProjectMenu"/> primary key.
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     Project version this menu is for
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }

    /// <summary>
    ///     Project menu items
    /// </summary>
    [Column(TypeName = "jsonb")]
    public LinkItem LinkItem { get; set; }
    
    /// <summary>
    ///     When was the last time this menu was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this menu created?
    /// </summary>
    public DateTime CreationTime { get; set; }
}