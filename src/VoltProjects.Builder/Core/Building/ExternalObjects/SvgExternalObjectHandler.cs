using System.Xml;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     External object handler for SVGs
/// </summary>
public sealed class SvgExternalObjectHandler : GenericExternalObject
{
    private readonly XmlDocument xmlDocument;
    
    /// <summary>
    ///     Creates a new <see cref="SvgExternalObjectHandler"/> instance
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="filePathRelativeToBuiltDocs"></param>
    /// <param name="projectName"></param>
    /// <param name="projectVersion"></param>
    public SvgExternalObjectHandler(string fullFilePath, string filePathRelativeToBuiltDocs, string projectName, string projectVersion)
        : base(fullFilePath, filePathRelativeToBuiltDocs, projectName, projectVersion)
    {
        xmlDocument = new XmlDocument();
        xmlDocument.Load(ObjectStream);
        
        XmlElement? root = xmlDocument.DocumentElement;
        if(root == null)
            throw new NullReferenceException("Failed to get svg root element");

        string widthValue = root.GetAttribute("width");
        string heightValue = root.GetAttribute("height");
        
        if(!string.IsNullOrWhiteSpace(widthValue))
            Width = root.GetAttribute("width");
        
        if (!string.IsNullOrWhiteSpace(heightValue))
            Height = root.GetAttribute("height");
    }
    
    public string? Width { get; private set; }
    public string? Height { get; private set; }
    
    public override async Task<Stream> GetUploadFileStream()
    {
        //Dispose of the old stream
        await ObjectStream.DisposeAsync();
            
        ObjectStream = new MemoryStream();
        xmlDocument.Save(ObjectStream);
        
        ObjectStream.Position = 0;
        
        return await base.GetUploadFileStream();
    }
}