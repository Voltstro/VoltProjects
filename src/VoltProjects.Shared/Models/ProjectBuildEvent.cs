using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A build event for a project
/// </summary>
[Table("project_build_event")]
public class ProjectBuildEvent
{
    /// <summary>
    ///     Generated primary key
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     What <see cref="ProjectVersion"/> this build event is for?
    /// </summary>
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion Project { get; set; }
    
    /// <summary>
    ///     Internal builder version number
    /// </summary>
    public int BuilderVer { get; set; }
    
    /// <summary>
    ///     Was this build event a success?
    /// </summary>
    public bool Successful { get; set; }
    
    /// <summary>
    ///     Any additional message to include
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    ///     What was the git hash that was used?
    /// </summary>
    [StringLength(41)]
    public string GitHash { get; set; }
    
    /// <summary>
    ///     When was the last time this build event was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this build event created
    /// </summary>
    public DateTime CreationTime { get; set; }
    
    public List<ProjectBuildEventLog> BuildEventLogs { get; set; }
}