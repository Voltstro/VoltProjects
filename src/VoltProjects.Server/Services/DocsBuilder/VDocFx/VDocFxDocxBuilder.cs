using System;
using System.Diagnostics;
using System.IO;

namespace VoltProjects.Server.Services.DocsBuilder.VDocFx;

/// <summary>
///     Custom DocFX builder for VoltProject
///     <para>Uses our custom theme </para>
/// </summary>
public sealed class VDocFxDocxBuilder : IDocsBuilder
{
    public string Name => "vdocfx";

    public void Build(string docsPath)
    {
        string docsConfigPath = Path.Combine(docsPath, "vdocfx.yml");
        if (!File.Exists(docsConfigPath))
            throw new FileNotFoundException($"vdocfx.yml file was not found in {docsPath}!");

        ProcessStartInfo startInfo = new("vdocfx", "build --output-type PageJson")
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