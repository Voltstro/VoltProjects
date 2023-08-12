using System.IO.Compression;
using Jint;
using Jint.Native;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Builder;

public class HtmlHighlightService
{
    private const string HlJsPath = "Data/highlight.js";
    private const string HlJsZip = "Data/highlight.zip";
    
    private readonly ILogger<HtmlHighlightService> logger;
    
    private readonly Engine engine;
    private readonly object parseLock;

    public HtmlHighlightService(ILogger<HtmlHighlightService> logger)
    {
        this.logger = logger;
        parseLock = new object();
        
        //Check if highlight.js exists
        if (!File.Exists(HlJsPath))
        {
            logger.LogWarning("highlight.js doesn't exist... extracting...");
            if (!File.Exists(HlJsZip))
                throw new FileNotFoundException("HLJs zip doesn't exist!");
            
            //Extracting time
            ZipFile.ExtractToDirectory(HlJsZip, "Data/");
        }
        
        //Read it
        string jsFile = File.ReadAllText(HlJsPath);
        
        //Create Jint Engine
        engine = new Engine();
        engine.Execute(jsFile);
    }

    public string ParseCodeBlock(string codeBlock, string? language)
    {
        //Need lock, Jint is not thread safe
        lock (parseLock)
        {
            engine.SetValue("input", codeBlock);
            engine.SetValue("languages", language != null ? new[] { language } : null);
            engine.Execute("highlighted = hljs.highlightAuto(input, languages)");
            JsValue parsedCodeBlock = engine.Evaluate("highlighted");
            return parsedCodeBlock.AsObject()["value"].AsString();
        }
    }
}