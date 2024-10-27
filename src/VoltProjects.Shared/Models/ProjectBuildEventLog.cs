using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_build_event_log")]
public class ProjectBuildEventLog
{
    [Key]
    public int Id { get; init; }
    
    public virtual ProjectBuildEvent BuildEvent { get; set; }
    
    [ForeignKey("BuildEvent")]
    public int BuildEventId { get; set; }
    
    public string Message { get; set; }
    
    public DateTime Date { get; set; }
    
    public virtual LogLevel LogLevel { get; set; }
    
    [ForeignKey("LogLevel")]
    public int LogLevelId { get; set; }
}