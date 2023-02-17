using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

[Table("ProjectLanguage")]
public class ProjectLanguage
{
    [Key]
    public int Id { get; init; }
    
    public int ProjectId { get; set; }
    public virtual Project Project { get; set; }
    
    public int LanguageId { get; set; }
    public virtual Language Language { get; set; }
}