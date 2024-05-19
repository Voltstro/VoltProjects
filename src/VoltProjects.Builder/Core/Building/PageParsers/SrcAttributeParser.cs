using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Builder.Services.Storage;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     <see cref="IPageParser"/> for attributes of src (like images and scripts)
/// </summary>
public class SrcAttributeParser : IPageParser
{
    private readonly ILogger logger;
    private readonly IStorageService storageService;

    private readonly SrcType[] srcTypesXPaths = [
        new SrcType("//img/@src", SrcObjectType.Image),
        new SrcType("//script/@src", SrcObjectType.Generic)];
    
    public SrcAttributeParser(ILogger logger, IStorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;
    }
    
    public List<IExternalObjectHandler>? FormatPage(string builtDocsLocation, ref ProjectPage page, ref HtmlDocument htmlDocument)
    {
        string pageCurrentPath = Path.Combine(builtDocsLocation, page.Path);

        List<IExternalObjectHandler> externalObjects = new();
        foreach (SrcType srcType in srcTypesXPaths)
        {
            HtmlNodeCollection? srcCollection = htmlDocument.DocumentNode.SelectNodes(srcType.XPath);
            if (srcCollection is { Count: > 0 })
            {
                foreach (HtmlNode htmlNode in srcCollection)
                {
                    HtmlAttribute srcAttribute = htmlNode.Attributes["src"];
                    string objectSrc = srcAttribute.Value;
                    
                    //Off-site object we do not care about
                    if(objectSrc.StartsWith("http"))
                        continue;

                    //Need to get the objects full path
                    string objectPath = Path.Combine(pageCurrentPath, objectSrc);
                    if (!File.Exists(objectPath))
                    {
                        logger.LogWarning("Could not found object on page {PageTitle} at location {Path}!", page.Title, objectPath);
                        continue;
                    }

                    string objectPathInProject = Path.GetRelativePath(builtDocsLocation, objectPath);
                    string fullObjectPath = Path.Combine(builtDocsLocation, objectPathInProject);

                    IExternalObjectHandler externalObject = srcType.ObjectType == SrcObjectType.Image
                        ? new ImageExternalObjectHandler(fullObjectPath, objectPathInProject, page) 
                        : new GenericExternalObject(fullObjectPath, objectPathInProject, page);
                    externalObjects.Add(externalObject);

                    //Set object src to uploaded URL
                    string fullUrl = storageService.GetFullUploadUrl(externalObject);
                    htmlNode.SetAttributeValue("src", fullUrl);
                }
            }
        }

        return externalObjects;
    }

    private record SrcType(string XPath, SrcObjectType ObjectType);
    
    private enum SrcObjectType
    {
        Generic,
        Image
    }
}