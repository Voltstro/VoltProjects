using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     Project menu item 
/// </summary>
[Table("project_menu_item")]
public class ProjectMenuItem
{
    /// <summary>
    ///     <see cref="ProjectMenuItem"/> primary key
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     <see cref="ProjectVersion.Id"/> this <see cref="ProjectMenuItem"/> belongs to
    /// </summary>
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     <see cref="ProjectVersion.Id"/> this <see cref="ProjectMenuItem"/> belongs to
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    
    /// <summary>
    ///     Href (path) to the page
    /// </summary>
    public string Href { get; set; }
    
    /// <summary>
    ///     Title displayed for the project menu item
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    ///     Order to display the project menu items in
    /// </summary>
    public int ItemOrder { get; set; }
}