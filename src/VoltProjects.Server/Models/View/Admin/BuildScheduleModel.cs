using VoltProjects.Server.Shared.Validation;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

/// <summary>
///     View model for build/schedules/edit{/}new/ page
/// </summary>
public class BuildScheduleModel : ProjectBuildSchedule
{
    public BuildScheduleModel()
    {
    }
    
    public BuildScheduleModel(ProjectBuildSchedule projectBuildSchedule)
    {
        Id = projectBuildSchedule.Id;
        ProjectVersionId = projectBuildSchedule.ProjectVersionId;
        Cron = projectBuildSchedule.Cron;
        LastExecuteTime = projectBuildSchedule.LastExecuteTime;
        IsActive = projectBuildSchedule.IsActive;
        IgnoreBuildEvents = projectBuildSchedule.IgnoreBuildEvents;
        LastUpdateTime = projectBuildSchedule.LastUpdateTime;
        CreationTime = projectBuildSchedule.CreationTime;
    }
    
    public new int? Id { get; set; }

    [Cron]
    public new string Cron { get; set; } = "0 0 0 */2 * *";
    
    public ProjectVersion[] ProjectVersions { get; set; }
    
    public bool? Success { get; set; }
}