using System.Reflection;
using System.Runtime.Loader;

namespace VoltProjects.DocsBuilder.Core.Assemblies;

internal class LoadContext : AssemblyLoadContext
{
    private readonly string basePath;
    
    internal LoadContext()
    {
        Resolving += OnResolving;
        basePath = AppContext.BaseDirectory;
    }

    private Assembly? OnResolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
    {
        string filePath = Path.GetFullPath($"{basePath}/{assemblyName.Name}.dll");
        return File.Exists(filePath) ? loadContext.LoadFromAssemblyPath(filePath) : null;
    }
}