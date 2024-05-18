using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.PageParsers;

/// <summary>
///     <see cref="IPageParser"/> for images
/// </summary>
public class ImageParser : IPageParser
{
    private readonly ILogger logger;
    
    public ImageParser(ILogger logger)
    {
        this.logger = logger;
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

                //BuildProjectImage image = new(config, page, imagePathInProject, fullImagePath);
                //projectImages.Add(image);

                //TODO: Set image path
                //imageNode.SetAttributeValue("src", image.FullImagePath);
            }

            return externalObjects;
        }

        return null;
    }
}