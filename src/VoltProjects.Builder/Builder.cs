using VoltProjects.Shared.Models;

namespace VoltProjects.Builder;

public abstract class Builder
{
    public abstract void PrepareBuilder(ref string[]? arguments, string docsPath, string docsBuiltPath);
    
    public abstract BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath);
}