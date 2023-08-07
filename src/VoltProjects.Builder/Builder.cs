using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder;

public abstract class Builder
{
    public abstract void BuildProject(ProjectVersion projectVersion, VoltProjectDbContext dbContext);
}