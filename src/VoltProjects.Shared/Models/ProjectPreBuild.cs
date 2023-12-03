using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     Command to execute before building the docs
/// </summary>
[Table("project_pre_build")]
public class ProjectPreBuild
{
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }

    /// <summary>
    ///     Sort order of this command
    /// </summary>
    public int Order { get; set; } = 1;

    /// <summary>
    ///     The command to execute
    /// </summary>
    public string Command { get; set; } = "dotnet";
    
    /// <summary>
    ///     Optional arguments to include
    /// </summary>
    public string? Arguments { get; set; } = "build";
    
    /// <summary>
    ///     When was the last time this TOC was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this TOC created?
    /// </summary>
    public DateTime CreationTime { get; set; }
}