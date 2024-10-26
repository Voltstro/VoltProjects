using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     An item to display that is attached to a <see cref="ProjectToc"/>
/// </summary>
[Table("project_toc_item")]
public class ProjectTocItem
{
    /// <summary>
    ///     Primary key for <see cref="ProjectTocItem"/>
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     The associated <see cref="ProjectToc.Id"/>
    /// </summary>
    public virtual ProjectToc ProjectToc { get; set; }
    
    /// <summary>
    ///     The associated <see cref="ProjectToc.Id"/>
    /// </summary>
    [ForeignKey("ProjectToc")]
    public int ProjectTocId { get; set; }
    
    /// <summary>
    ///     The <see cref="ProjectVersion.Id"/> this item is for
    /// </summary>
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     The <see cref="ProjectVersion.Id"/> this item is for
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    
    /// <summary>
    ///     Title to display for the toc item
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    ///     Href (path) to page
    /// </summary>
    public string? Href { get; set; }
    
    /// <summary>
    ///     Order to display this item in
    /// </summary>
    public int ItemOrder { get; set; }
    
    /// <summary>
    ///     Parent <see cref="ProjectTocItem.Id"/> this one sit under
    /// </summary>
    public virtual ProjectTocItem? ParentTocItem { get; set; }
    
    /// <summary>
    ///     Parent <see cref="ProjectTocItem.Id"/> this one sit under
    /// </summary>
    [ForeignKey("ParentTocItem")]
    public int? ParentTocItemId { get; set; }
}