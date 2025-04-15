using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Builder.Services;

/// <summary>
///     Highlights HTML code blocks with hljs
/// </summary>
public sealed class HtmlHighlightService
{
    private readonly ILogger<HtmlHighlightService> logger;
    
    private readonly Engine engine;
    private readonly JsValue hljsEntry;
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
        
        //Get internal hljs script and read it
        using Stream? hljsScriptStream = typeof(HtmlHighlightService).Assembly.GetManifestResourceStream("HighlightJs/index.js");
        if (hljsScriptStream == null)
            throw new FileNotFoundException("Failed to load HighlightJs script!");
        
        using StreamReader hljsReader = new(hljsScriptStream);
        string hljsSrc = hljsReader.ReadToEnd();
        
        //Create Jint Engine
        engine = new Engine(options =>
        {
            options.Strict = true;
        });
        
        //Create the module and get the entry function
        engine.Modules.Add("hljs", 
            x => x
                .WithOptions(_ => new ModuleParsingOptions
                {
                    CompileRegex = true,
                    Tolerant = true
                })
                .AddSource(hljsSrc));
        ObjectInstance module = engine.Modules.Import("hljs");
        hljsEntry = module.Get("default");
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
                JsValue inputValue =  JsValue.FromObject(engine, codeBlock);
                JsValue languageValue = JsValue.FromObject(engine, language != null ? new[] { language } : null);

                JsValue result = hljsEntry.Call(inputValue, languageValue);
                string codeBlockHighlighted = result.AsObject()["value"].AsString();
                return codeBlockHighlighted;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while highlighting code block!");
            return codeBlock;
        }
    }
}