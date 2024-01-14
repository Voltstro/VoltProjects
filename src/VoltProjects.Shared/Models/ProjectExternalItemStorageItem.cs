using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_external_item_storage_item")]
public class ProjectExternalItemStorageItem
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectExternalItem")]
    public int ProjectExternalItemId { get; set; }
    public virtual ProjectExternalItem ProjectExternalItem { get; set; }
    
    [ForeignKey("StorageItem")]
    public int StorageItemId { get; set; }
    public virtual ProjectStorageItem StorageItem { get; set; }
    
}