using System.Diagnostics;
using VoltProjects.DocsBuilder.Core;

namespace VoltProjects.DocsBuilder.DocFx;

/// <summary>
///     Custom DocFX builder for VoltProject
///     <para>Uses our custom theme </para>
/// </summary>
public sealed class DocFxDocxBuilder : IDocsBuilder
{
    public string Name => "DocFx";

    private readonly string templatePath;
    
    public DocFxDocxBuilder()
    {
        templatePath = Path.Combine(AppContext.BaseDirectory, "template");
        if (!Directory.Exists(templatePath))
            throw new DirectoryNotFoundException("DocFx's custom template was not found!");
    }

    public void Build(string docsPath)
    {
        string docsConfigPath = Path.Combine(docsPath, "docfx.json");
        if (!File.Exists(docsConfigPath))
            throw new FileNotFoundException($"docfx.json file was not found in {docsPath}!");

        ProcessStartInfo startInfo = new("docfx", $"build \"{docsConfigPath}\" --template \"default,statictoc,{templatePath}\"")
        {
            WorkingDirectory = docsPath
        };

        Process process = new();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception("Docfx failed to exit cleanly!");
        
        process.Dispose();
    }
}