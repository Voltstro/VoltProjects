using System.Diagnostics;
using System.Runtime.InteropServices;
using VoltProjects.DocsBuilder.Core;

namespace VoltProjects.DocsBuilder.DocFx;

public class DocFxDocxBuilder : IDocsBuilder
{
    private const string DocfxDefaultAppName = "docfx";
    
    public string Name => "DocFx";

    public void Build(string docsPath)
    {
        string docfxName = DocfxDefaultAppName;
        
        //Windows we need the '.exe'
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            docfxName += ".exe";
        
        //Environment variables overrides everything
        string? docfxPath = Environment.GetEnvironmentVariable("VOLTPRJ_DOCFX");
        if (!string.IsNullOrWhiteSpace(docfxPath))
            docfxName = docfxPath;

        //Run DocFx
        Process docfxProcess = new()
        {
            StartInfo = new ProcessStartInfo(docfxName, "build")
            {
                WorkingDirectory = Path.GetFullPath(docsPath),
                UseShellExecute = true
            }
        };
        docfxProcess.Start();
        docfxProcess.WaitForExit();

        if (docfxProcess.ExitCode != 0)
            throw new ApplicationException("DocFx process failed to build the docs site!");
        
        docfxProcess.Kill(true);
        docfxProcess.Dispose();
    }
}