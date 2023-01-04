using System.Diagnostics;
using VoltProjects.DocsBuilder.Core;

namespace VoltProjects.DocsBuilder.DocFx;

public sealed class DocFxDocxBuilder : IDocsBuilder
{
    public string Name => "DocFx";

    public void Build(string docsPath)
    {
        string docsConfigPath = Path.Combine(docsPath, "docfx.json");
        if (!File.Exists(docsConfigPath))
            throw new FileNotFoundException($"docfx.json file was not found in {docsPath}!");

        ProcessStartInfo startInfo = new("docfx", $"build \"{docsConfigPath}\"")
        {
            WorkingDirectory = docsPath
        };

        Process process = new();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception("Docfx failed to exit cleanly!");
    }
}