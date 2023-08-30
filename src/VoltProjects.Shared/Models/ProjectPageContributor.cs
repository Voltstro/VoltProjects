using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VoltProjects.Shared.Models;

[Table("project_page_contributor")]
public class ProjectPageContributor
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("Page")]
    public int PageId { get; set; }
    public virtual ProjectPage Page { get; set; }
    
    public string GitHubUserId { get; set; }
}