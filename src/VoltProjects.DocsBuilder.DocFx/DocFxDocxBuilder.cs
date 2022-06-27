using System.Diagnostics;
using System.Runtime.InteropServices;
using VoltProjects.DocsBuilder.Core;

namespace VoltProjects.DocsBuilder.DocFx;

public class DocFxDocxBuilder : IDocsBuilder
{
    private const string DocfxAppName = "docfx";
    
    public string Name => "DocFx";

    public void Build(string docsPath)
    {
        string docfxPath = $"{AppContext.BaseDirectory}/{DocfxAppName}";
        
        //Windows we need the '.exe'
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            docfxPath += ".exe";

        docfxPath = Path.GetFullPath(docfxPath);
        if (!File.Exists(docfxPath))
            throw new FileNotFoundException("DocFx was not found!");

        //Run DocFx
        Process docfxProcess = new()
        {
            StartInfo = new ProcessStartInfo(docfxPath, "build")
            {
                WorkingDirectory = docsPath
            }
        };
        docfxProcess.Start();
        docfxProcess.WaitForExit();

        if (docfxProcess.ExitCode != 0)
            throw new ApplicationException("DocFx process failed to build the docs site!");
    }
}