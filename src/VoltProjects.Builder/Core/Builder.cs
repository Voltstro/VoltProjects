using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Base class for all Doc Builders to implement
/// </summary>
public abstract class Builder
{
    /// <summary>
    ///     Method is called before the build process is executed. Arguments should be formatted, and anything the builder might need should be setup
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="docsPath"></param>
    /// <param name="docsBuiltPath"></param>
    public abstract void PrepareBuilder(ref string[]? arguments, string docsPath, string docsBuiltPath);
    
    /// <summary>
    ///     Process everything and convert into a standard VoltProjects format
    /// </summary>
    /// <param name="projectVersion"></param>
    /// <param name="docsPath"></param>
    /// <param name="docsBuiltPath"></param>
    /// <returns></returns>
    public abstract BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath);
}