using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("project_build_schedule")]
public class ProjectBuildSchedule
{
    [Key]
    public int Id { get; init; }
    
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }

    /// <summary>
    ///     Cron time for this build schedule
    /// </summary>
    public string Cron { get; set; }

    /// <summary>
    ///     Last time this build schedule was executed
    /// </summary>
    public DateTime? LastExecuteTime { get; set; }
    
    /// <summary>
    ///     Is this build schedule active?
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    ///     Will ignore build events and build event if the last build for that git commit was a success
    /// </summary>
    public bool IgnoreBuildEvents { get; set; }
    
    /// <summary>
    ///     When was the last time this build schedule was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this build schedule created
    /// </summary>
    public DateTime CreationTime { get; set; }
}