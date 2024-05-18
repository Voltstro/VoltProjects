using HtmlAgilityPack;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     Formats or parses stuff on a <see cref="ProjectPage"/>
/// </summary>
public interface IPageParser
{
    /// <summary>
    ///     Format a page by using HtmlAgilityPack's <see cref="HtmlDocument"/>
    ///     <para>If external objects need to be uploaded based of the page, return a list of <see cref="IExternalObjectHandler"/></para>
    /// </summary>
    /// <param name="builtDocsLocation"></param>
    /// <param name="page"></param>
    /// <param name="htmlDocument"></param>
    /// <returns></returns>
    public List<IExternalObjectHandler>? FormatPage(string builtDocsLocation, ref ProjectPage page, ref HtmlDocument htmlDocument);
}