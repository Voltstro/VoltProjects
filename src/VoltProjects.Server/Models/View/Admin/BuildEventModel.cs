using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

public class BuildEventModel : ProjectBuildEvent
{
    public BuildEventModel(ProjectBuildEvent buildEvent, PagedResult<ProjectBuildEventLog> buildEventLogs)
    {
        Id = buildEvent.Id;
        ProjectVersionId = buildEvent.ProjectVersionId;
        Project = buildEvent.Project;
        BuilderVer = buildEvent.BuilderVer;
        Successful = buildEvent.Successful;
        Message = buildEvent.Message;
        GitHash = buildEvent.GitHash;
        LastUpdateTime = buildEvent.LastUpdateTime;
        CreationTime = buildEvent.CreationTime;
        BuildEventLogs = buildEventLogs;
    }
    
    public new PagedResult<ProjectBuildEventLog> BuildEventLogs { get; }
}