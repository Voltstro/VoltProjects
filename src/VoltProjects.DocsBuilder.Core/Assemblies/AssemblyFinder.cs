using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace VoltProjects.DocsBuilder.Core.Assemblies;

//Based of Serilog's DependencyContextAssemblyFinder
//https://github.com/serilog/serilog-settings-configuration/blob/dev/src/Serilog.Settings.Configuration/Settings/Configuration/Assemblies/DependencyContextAssemblyFinder.cs
internal class AssemblyFinder
{
    private readonly DependencyContext dependencyContext;
    
    public AssemblyFinder(DependencyContext dependencyContext)
    {
        this.dependencyContext = dependencyContext ?? throw new ArgumentNullException(nameof(dependencyContext));
    }
    
    public IEnumerable<AssemblyName> FindAssembliesContainingName(string nameToFind)
    {
        IEnumerable<AssemblyName>? query = from library in dependencyContext.RuntimeLibraries
            where IsReferencingDocsBuilder(library)
            from assemblyName in library.GetDefaultAssemblyNames(dependencyContext)
            where IsCaseInsensitiveMatch(assemblyName.Name, nameToFind)
            select assemblyName;

        return query.ToList().AsReadOnly();
            
        static bool IsReferencingDocsBuilder(Library library)
        {
            const string docsBuilder = "voltprojects.docsbuilder.core";
            return library.Dependencies.Any(dependency =>
                dependency.Name.StartsWith(docsBuilder, StringComparison.OrdinalIgnoreCase) &&
                (dependency.Name.Length == docsBuilder.Length || dependency.Name[docsBuilder.Length] == '.'));
        }
        
        static bool IsCaseInsensitiveMatch(string? text, string textToFind)
        {
            return text != null && text.ToLowerInvariant().Contains(textToFind.ToLowerInvariant());
        }
    }
}