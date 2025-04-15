using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

public class BuildProjectResult
{
    public List<ProjectBuildEventLog> BuildEventLogs { get; set; }
}