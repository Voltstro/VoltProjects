using HtmlAgilityPack;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     <see cref="IPageParser"/> for extracting some form of a description from a page
/// </summary>
public sealed class DescriptionParser : IPageParser
{
    public List<IExternalObjectHandler>? FormatPage(string builtDocsLocation, ref ProjectPage page, ref HtmlDocument htmlDocument)
    {
        //Get first p block to get page description from
        HtmlNode? node = htmlDocument.DocumentNode.SelectSingleNode("//p[not(*)]");
        page.Description = node?.InnerText ?? page.Title;
        return null;
    }
}