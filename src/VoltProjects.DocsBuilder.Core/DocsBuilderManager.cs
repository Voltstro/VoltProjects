using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using VoltProjects.DocsBuilder.Core.Assemblies;

namespace VoltProjects.DocsBuilder.Core;

/// <summary>
///     Manager for <see cref="IDocsBuilder"/>
/// </summary>
public sealed class DocsBuilderManager
{
    private readonly IDocsBuilder[] docsBuilders;
    
    /// <summary>
    ///     Creates a new <see cref="DocsBuilderManager"/> instance
    /// </summary>
    /// <param name="dependencyContext">The <see cref="DependencyContext"/> to find <see cref="IDocsBuilder"/> from.</param>
    public DocsBuilderManager(DependencyContext dependencyContext)
    {
        List<IDocsBuilder> foundBuilders = new();
        
        AssemblyFinder assemblyFinder = new(dependencyContext);
        LoadContext loadContext = new();
        
        //Get all assemblies that have "voltprojects.docsbuilder" in their name
        IEnumerable<AssemblyName> assemblies = assemblyFinder.FindAssembliesContainingName("voltprojects.docsbuilder");
        foreach (AssemblyName assemblyName in assemblies)
        {
            //Load assembly
            Assembly assembly = loadContext.LoadFromAssemblyName(assemblyName);
            foreach (Type type in assembly.GetTypes().Where(x => x is { IsClass: true, IsPublic: true }))
            {
                //For each type, check if it is a IDocsBuilder
                if(!typeof(IDocsBuilder).IsAssignableFrom(type))
                    continue;

                //Create instance
                if(Activator.CreateInstance(type) is IDocsBuilder docsBuilder)
                    foundBuilders.Add(docsBuilder);
            }
        }

        //Create static array
        docsBuilders = foundBuilders.ToArray();
    }
    
    /// <summary>
    ///     Builds docs from a path
    /// </summary>
    /// <param name="docsBuilder"></param>
    /// <param name="docsPath"></param>
    /// <exception cref="DocsBuilderNotFoundException"></exception>
    public void BuildDocs(string docsBuilder, string docsPath)
    {
        //Find docs builder
        IDocsBuilder? builder = docsBuilders.FirstOrDefault(x => x.Name.Equals(docsBuilder, StringComparison.InvariantCultureIgnoreCase));
        if (builder == null)
            throw new DocsBuilderNotFoundException($"The builder {docsBuilder} was not found!");

        //Pass it over to the docs builder
        builder.Build(docsPath);
    }
}