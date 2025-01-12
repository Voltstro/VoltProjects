using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

public class ProjectsPageModel
{
    public PagedResult<Project> Projects { get; init; }
}