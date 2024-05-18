using SixLabors.ImageSharp;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     Image variant of <see cref="GenericExternalObject"/>.
///     <para>Converts any image type to webp</para>
/// </summary>
public sealed class ImageExternalObjectHandler : GenericExternalObject
{
    /// <summary>
    ///     Creates a new <see cref="ImageExternalObjectHandler"/> instance
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="filePathRelativeToBuiltDocs"></param>
    /// <param name="projectPage"></param>
    public ImageExternalObjectHandler(string fullFilePath, string filePathRelativeToBuiltDocs, ProjectPage projectPage)
        : base(fullFilePath, Path.ChangeExtension(filePathRelativeToBuiltDocs, ".webp"), projectPage)
    {
    }

    public override async Task<Stream> GetUploadFileStream()
    {
        if (ContentType != "image/webp")
        {
            //Convert image to webp
            Image image = await Image.LoadAsync(ObjectStream);
            
            //Dispose of the old stream
            await ObjectStream.DisposeAsync();
            
            ObjectStream = new MemoryStream();
            await image.SaveAsWebpAsync(ObjectStream);
            image.Dispose();

            ObjectStream.Position = 0;
        }
        
        return await base.GetUploadFileStream();
    }
}