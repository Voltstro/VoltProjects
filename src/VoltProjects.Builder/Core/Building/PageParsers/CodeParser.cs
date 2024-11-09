using System.Web;
using HtmlAgilityPack;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Builder.Services;
using VoltProjects.Shared.Models;
using VoltProjects.Shared.Services.Storage;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     <see cref="IPageParser"/> for code blocks
/// </summary>
public sealed class CodeParser : IPageParser
{
    private readonly HtmlHighlightService highlightService;
    
    private static readonly string[] SupportedLangs = ["csharp"];
    
    public CodeParser(HtmlHighlightService highlightService)
    {
        this.highlightService = highlightService;
    }
    
    public void FormatPage(string builtDocsLocation, ProjectPage page, ref List<IExternalObjectHandler> externalObjects, ref HtmlDocument htmlDocument)
    {
        HtmlNodeCollection? codeBlocks = htmlDocument.DocumentNode.SelectNodes("//pre/code");
        if (codeBlocks is not { Count: > 0 })
            return;
        
        foreach (HtmlNode codeBlock in codeBlocks)
        {
            string? text = codeBlock.InnerHtml;
            string? language = null;

            //Try to pick-up on the language
            HtmlAttribute? languageAttribute = codeBlock.Attributes["class"];
            if (languageAttribute != null)
            {
                language = languageAttribute.Value.Replace("lang-", "");
                if (!SupportedLangs.Contains(language))
                    language = null; //use autodetect
            }

            if (text != null)
            {
                string parsedCodeBlock =
                    highlightService.ParseCodeBlock(HttpUtility.HtmlDecode(text), language);
                codeBlock.InnerHtml = parsedCodeBlock;
            }

            codeBlock.SetAttributeValue("class", "hljs shadow");
        }
    }
}