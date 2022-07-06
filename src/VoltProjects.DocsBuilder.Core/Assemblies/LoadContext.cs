using System.Reflection;
using System.Runtime.Loader;

namespace VoltProjects.DocsBuilder.Core.Assemblies;

internal class LoadContext : AssemblyLoadContext
{
    private readonly string _basePath;
    
    internal LoadContext()
    {
        Resolving += OnResolving;
        _basePath = AppContext.BaseDirectory;
    }

    private Assembly? OnResolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
    {
        string filePath = Path.GetFullPath($"{_basePath}/{assemblyName.Name}.dll");
        return File.Exists(filePath) ? loadContext.LoadFromAssemblyPath(filePath) : null;
    }
}