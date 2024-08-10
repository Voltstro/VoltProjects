using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Jint;
using Jint.Native;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Builder.Services;

/// <summary>
///     Highlights HTML code blocks with hljs
/// </summary>
public sealed class HtmlHighlightService
{
    private const string HlJsPath = "Data/highlight.js";
    private const string HlJsZip = "Data/highlight.zip";
    private const string HlJsFileHash = "d3b7a8e4fb3c86f214a1bdbad45e7d52";
    
    private readonly ILogger<HtmlHighlightService> logger;
    
    private readonly Engine engine;
    private readonly object parseLock;

    /// <summary>
    ///     Creates a new <see cref="HtmlHighlightService"/> instance
    /// </summary>
    /// <param name="logger"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public HtmlHighlightService(ILogger<HtmlHighlightService> logger)
    {
        this.logger = logger;
        parseLock = new object();

        //Check if highlight.js exists
        if (!File.Exists(HlJsPath))
        {
            logger.LogWarning("highlight.js doesn't exist... extracting...");
            ExtractZip();
        }

        //Read it
        string jsFile = File.ReadAllText(HlJsPath);

        //Check hash of js file
        byte[] jsFileBytes = Encoding.UTF8.GetBytes(jsFile);
        byte[] jsFileHashBytes = MD5.HashData(jsFileBytes);
        string jsFileHash = BitConverter.ToString(jsFileHashBytes).Replace("-", string.Empty).ToLowerInvariant();

        if (jsFileHash != HlJsFileHash)
        {
            this.logger.LogWarning("highlight.js file does not have excepted hash. Most like needs updating. Re-extracting...");
            ExtractZip();

            //Re-read
            jsFile = File.ReadAllText(HlJsPath);
        }

        //Create Jint Engine
        engine = new Engine();
        engine.Execute(jsFile);
    }

    /// <summary>
    ///     Highlights HTML code blocks
    ///     <para>If an error occurs, the original HTML will be returned</para>
    /// </summary>
    /// <param name="codeBlock"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public string ParseCodeBlock(string codeBlock, string? language)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while highlighting code block!");
            return codeBlock;
        }
    }

    private void ExtractZip()
    {
        if (!File.Exists(HlJsZip))
            throw new FileNotFoundException("HLJs zip doesn't exist!");
            
        //Extracting time
        ZipFile.ExtractToDirectory(HlJsZip, "Data/", true);
    }
}