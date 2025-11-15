using SixLabors.ImageSharp;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     External object handler for images, E.G: PNGs, JPEG.
///     <para>Will convert any image to a webp on upload</para>
/// </summary>
public sealed class ImageExternalObjectHandler : GenericExternalObject
{
    private readonly Image image;
    
    /// <summary>
    ///     Creates a new <see cref="ImageExternalObjectHandler"/> instance
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="filePathRelativeToBuiltDocs"></param>
    /// <param name="projectName"></param>
    /// <param name="projectVersion"></param>
    public ImageExternalObjectHandler(string fullFilePath, string filePathRelativeToBuiltDocs, string projectName, string projectVersion)
        : base(fullFilePath, filePathRelativeToBuiltDocs, projectName, projectVersion)
    {
        image = Image.Load(ObjectStream);
    }
    
    public int Width => image.Width;
    public int Height => image.Height;

    public override async Task<Stream> GetUploadFileStream()
    {
        //Dispose of the old stream
        await ObjectStream.DisposeAsync();
            
        ObjectStream = new MemoryStream();
        await image.SaveAsWebpAsync(ObjectStream);
        image.Dispose();

        ObjectStream.Position = 0;
        
        return await base.GetUploadFileStream();
    }
}