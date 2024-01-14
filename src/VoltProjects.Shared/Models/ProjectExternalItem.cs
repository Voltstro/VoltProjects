using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     An external item that will be uploaded to cloud storage
/// </summary>
[Table("project_external_item")]
public class ProjectExternalItem
{
    /// <summary>
    ///     Primary Key
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     Project this item belongs to
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     Path of the file to upload
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     When was this page last updated?
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this page created?
    /// </summary>
    public DateTime CreationTime { get; set; }
}