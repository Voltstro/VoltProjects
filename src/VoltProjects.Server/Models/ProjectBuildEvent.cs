using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

[Table("ProjectBuildEvent")]
public class ProjectBuildEvent
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion Project { get; set; }
    
    [StringLength(41)]
    public string GitHash { get; set; }
    
    public DateTime Date { get; set; }
}