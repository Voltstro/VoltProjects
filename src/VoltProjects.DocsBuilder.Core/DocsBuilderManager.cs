using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyModel;
using VoltProjects.DocsBuilder.Core.Assemblies;

namespace VoltProjects.DocsBuilder.Core;

public sealed class DocsBuilderManager
{
    private const string DocsBuilderConfigFileName = "VoltDocsBuilder.json";

    private readonly List<IDocsBuilder> _docsBuilders;
    
    public DocsBuilderManager(DependencyContext dependencyContext)
    {
        _docsBuilders = new List<IDocsBuilder>();
        AssemblyFinder assemblyFinder = new(dependencyContext);
        LoadContext loadContext = new LoadContext();
        IEnumerable<AssemblyName> assemblies = assemblyFinder.FindAssembliesContainingName("voltprojects.docsbuilder");
        foreach (AssemblyName assemblyName in assemblies)
        {
            Assembly assembly = loadContext.LoadFromAssemblyName(assemblyName);
            foreach (Type type in assembly.GetTypes().Where(x => x.IsClass && x.IsPublic))
            {
                if(!typeof(IDocsBuilder).IsAssignableFrom(type))
                    continue;

                if(Activator.CreateInstance(type) is IDocsBuilder docsBuilder)
                    _docsBuilders.Add(docsBuilder);
            }
        }
    }
    
    public void BuildDocs(string projectPath, string docsPath)
    {
        string configFilePath = $"{docsPath}/{DocsBuilderConfigFileName}";
        
        //Config don't exist
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException($"{DocsBuilderConfigFileName} was not found!");

        //Read it
        DocsBuilderConfig? config = JsonSerializer.Deserialize<DocsBuilderConfig>(File.ReadAllText(configFilePath));
        if (config == null)
            throw new JsonException("Fail to read json!");

        //Find docs builder
        string docsType = config.DocsType;
        IDocsBuilder? builder = _docsBuilders.FirstOrDefault(x => x.Name == docsType);
        if (builder == null)
            throw new DocsBuilderNotFoundException();
        
        //Run pre-actions
        foreach (DocsBuilderAction action in config.PreActions)
        {
            Process actionProcess = new()
            {
                StartInfo = new ProcessStartInfo(action.Program, action.Arguments)
                {
                    WorkingDirectory = projectPath
                }
            };
            actionProcess.Start();
            actionProcess.WaitForExit();

            if (actionProcess.ExitCode != 0)
                throw new Exception("Action process failed to run!");
            
            actionProcess.Kill(true);
            actionProcess.Dispose();
        }
        
        //Pass it over to the docs builder
        builder.Build(docsPath);
    }
}