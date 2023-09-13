using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     Item that is stored on cloud storage item
/// </summary>
[Table("project_storage_item")]
public class ProjectStorageItem
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     Path to this item
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     Hash of the item
    /// </summary>
    public string Hash { get; set; }
    
    /// <summary>
    ///     When was this item created
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    /// <summary>
    ///     When was this page last updated?
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}