using System.Collections.Generic;
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
        IFileInfo menuJson = fileProvider.GetFileInfo(Path.Combine(sitePath, "menu.json"));
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
        
        //Toc
        VDocFxViewModel.TocItem? tocItem = null;
        if (pageJson.Metadata.TocRel != null)
        {
            IFileInfo menuToc = fileProvider.GetFileInfo(Path.Combine(sitePath, filePath, pageJson.Metadata.TocRel));
            if (menuToc.Exists)
            {
                VDocFxTocJson tocJsonObj =
                    JsonSerializer.Deserialize<VDocFxTocJson>(File.ReadAllText(menuToc.PhysicalPath));
                
                string rel = pageJson.Metadata.TocRel.Replace("toc.json", "");
                tocItem = BuildItem(tocJsonObj, rel);
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
                    MenuItems = menuItems,
                    Toc = tocItem
                }
            }
        };
        return view;
    }

    private VDocFxViewModel.TocItem BuildItem(VDocFxTocJson tocJson, string rel)
    {
        VDocFxViewModel.TocItem[]? children = null;
        if (tocJson.Items != null)
        {
            children = new VDocFxViewModel.TocItem[tocJson.Items.Length];
            for (int i = 0; i < tocJson.Items.Length; i++)
            {
                children[i] = BuildItem(tocJson.Items[i], rel);
            }
        }

        string? href = tocJson.Href;
        if (href != null)
        {
            if (!href.StartsWith("/"))
                href = Path.Combine(rel, href);
        }

        return new VDocFxViewModel.TocItem(tocJson.Name, href, false, children);
    }
}