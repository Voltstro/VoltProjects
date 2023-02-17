using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using VoltProjects.Server.Models.View;

namespace VoltProjects.Server.Services.DocsView.VDocFx;

public class VDocFxView : IDocView
{
    public string Name => "vdocfx";

    public ViewResult? GetViewFromFile(ViewDataDictionary viewData, IFileProvider fileProvider, string sitePath, string potentialFile)
    {
        string fileName = Path.GetFileNameWithoutExtension(potentialFile);
        string? filePath = Path.GetDirectoryName(potentialFile);

        string jsonFile = Path.Combine(sitePath, filePath, $"{fileName}.raw.page.json");
        IFileInfo file = fileProvider.GetFileInfo(jsonFile);
        if (!file.Exists)
            return null;
        
        string? fullFilePath = file.PhysicalPath;
        if (string.IsNullOrWhiteSpace(fullFilePath))
            return null;

        VDocFxPageJson? pageJson = JsonSerializer.Deserialize<VDocFxPageJson>(File.ReadAllText(fullFilePath));
        if (pageJson == null)
            return null;

        ViewResult view = new()
        {
            ViewName = "~/Views/Docs/VDocFxView.cshtml",
            ViewData = new ViewDataDictionary(viewData)
            {
                Model = new VDocFxViewModel
                {
                    Tile = pageJson.Metadata.Title,
                    RawTitle = pageJson.Metadata.RawTitle,
                    Content = pageJson.Content
                }
            }
        };
        return view;
    }
}