using VoltProjects.Shared.Models;

namespace VoltProjects.Builder;

public abstract class Builder
{
    public abstract void RunBuildProcess(string docsPath, string docsBuiltPath);
    
    public abstract BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath);
}