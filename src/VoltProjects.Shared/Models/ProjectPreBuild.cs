using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("ProjectPreBuild")]
public class ProjectPreBuild
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    public string Command { get; set; }
    public string? Arguments { get; set; }
}