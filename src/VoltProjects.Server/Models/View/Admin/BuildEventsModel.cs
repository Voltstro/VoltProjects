using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

public class BuildEventsModel
{
    public PagedResult<ProjectBuildEvent> ProjectBuildEvents { get; init; }
}