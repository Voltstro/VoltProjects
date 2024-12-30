using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

/// <summary>
///     View model for build/schedules/ page
/// </summary>
public class BuildSchedulesModel
{
    public ProjectBuildSchedule[] BuildSchedules { get; init; }
}