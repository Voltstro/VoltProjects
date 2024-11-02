using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_page_breadcrumb")]
public class ProjectPageBreadcrumb
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectPage")]
    public int ProjectPageId { get; set; }
    
    public virtual ProjectPage ProjectPage { get; set; }
    
    public string Title { get; set; }
    
    public string? Href { get; set; }
    
    public int BreadcrumbOrder { get; set; }
}