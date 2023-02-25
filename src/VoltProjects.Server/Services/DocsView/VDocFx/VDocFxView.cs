using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;

namespace VoltProjects.Server.Services.DocsView.VDocFx;

public class VDocFxView : IDocView
{
    public string Name => "vdocfx";

    public ViewResult? GetViewFromFile(ViewDataDictionary viewData, IFileProvider fileProvider, string sitePath, string potentialFile, string project)
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

        //Main json
        VDocFxPageJson? pageJson = JsonSerializer.Deserialize<VDocFxPageJson>(File.ReadAllText(fullFilePath));
        if (pageJson == null)
            return null;

        string layout = pageJson.Metadata.Layout.ToLower();
        string? pageType = pageJson.Metadata.PageType;
        if (pageType == null)
            pageType = layout;
        if (pageType == "dotnet")
        {
            pageType = "reference";
            layout = "conceptual";
        }

        //Menu
        VDocFxViewModel.MenuItem[]? menuItems = null;
        IFileInfo menuJson = fileProvider.GetFileInfo(Path.Join(sitePath, "menu.json"));
        if (menuJson.Exists)
        {
            VDocfxMenuJson menuJsonObj =
                JsonSerializer.Deserialize<VDocfxMenuJson>(File.ReadAllText(menuJson.PhysicalPath));
            menuItems = new VDocFxViewModel.MenuItem[menuJsonObj.Items.Length];
            for (int i = 0; i < menuJsonObj.Items.Length; i++)
            {
                menuItems[i] = new VDocFxViewModel.MenuItem(menuJsonObj.Items[i].Name, $"/{sitePath}/{menuJsonObj.Items[i].Href}");
            }
        }
        
        ViewResult view = new()
        {
            ViewName = "~/Views/Docs/VDocFxView.cshtml",
            ViewData = new ViewDataDictionary(viewData)
            {
                Model = new VDocFxViewModel
                {
                    ProjectName = project,
                    ProjectBasePath = $"/{sitePath}/",
                    Tile = pageJson.Metadata.Title,
                    RawTitle = pageJson.Metadata.RawTitle,
                    Content = pageJson.Content,
                    Layout = layout,
                    PageType = pageType,
                    GitUrl = pageJson.Metadata.GitUrl,
                    MenuItems = menuItems
                }
            }
        };
        return view;
    }
}