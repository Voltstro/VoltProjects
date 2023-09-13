using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A <see cref="ProjectStorageItem"/> that a <see cref="ProjectPage"/> is using
/// </summary>
[Table("project_page_storage_item")]
public class ProjectPageStorageItem
{
    /// <summary>
    ///     Generated PK
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     Page Ref ID
    /// </summary>
    [ForeignKey("Page")]
    public int PageId { get; set; }
    public virtual ProjectPage Page { get; set; }
    
    /// <summary>
    ///     Storage Item Ref ID
    /// </summary>
    [ForeignKey("StorageItem")]
    public int StorageItemId { get; set; }
    public virtual ProjectStorageItem StorageItem { get; set; }
}