using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

[Table("ProjectVersion")]
public class ProjectVersion
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }
    
    [Required]
    public string VersionTag { get; set; }

    [ForeignKey("DocBuilder")]
    public string DocBuilderId { get; set; }
    public virtual DocBuilder DocBuilder { get; set; }
    
    [ForeignKey("DocView")]
    public string DocViewId { get; set; }
    public virtual DocView DocView { get; set; }
    
    [Required]
    public string DocsPath { get; set; }
    
    [Required]
    public string DocsBuiltPath { get; set; }
    
    public bool IsDefault { get; set; }
}