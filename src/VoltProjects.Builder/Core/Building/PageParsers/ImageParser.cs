using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Builder.Services.Storage;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     <see cref="IPageParser"/> for images
/// </summary>
public class ImageParser : IPageParser
{
    private readonly ILogger logger;
    private IStorageService storageService;
    
    public ImageParser(ILogger logger, IStorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;
    }
    
    public List<IExternalObjectHandler>? FormatPage(string builtDocsLocation, ref ProjectPage page, ref HtmlDocument htmlDocument)
    {
        string pageCurrentPath = Path.Combine(builtDocsLocation, page.Path);
        
        HtmlNodeCollection? images = htmlDocument.DocumentNode.SelectNodes("//img/@src");
        if (images is { Count: > 0 })
        {
            List<IExternalObjectHandler> externalObjects = new();
            foreach (HtmlNode imageNode in images)
            {
                HtmlAttribute srcAttribute = imageNode.Attributes["src"];
                    
                //Get image file
                string imageSrc = srcAttribute.Value;
                    
                //Off-Site Image, don't care about it
                if(imageSrc.StartsWith("http"))
                    continue;

                string imagePath = Path.Combine(pageCurrentPath, imageSrc);
                if (!File.Exists(imagePath))
                {
                    logger.LogWarning("Could not found image on page {PageTitle} at location {Path}!", page.Title, imagePath);
                    continue;
                }

                string imagePathInProject = Path.GetRelativePath(builtDocsLocation, imagePath);
                string fullImagePath = Path.Combine(builtDocsLocation, imagePathInProject);

                ImageExternalObjectHandler externalObject = new(fullImagePath, imagePathInProject, page);
                externalObjects.Add(externalObject);

                //Set image src to uploaded URL
                string fullUrl = storageService.GetFullUploadUrl(externalObject);
                imageNode.SetAttributeValue("src", fullUrl);
            }

            return externalObjects;
        }

        return null;
    }
}