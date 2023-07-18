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

    public ViewResult? GetViewFromFile(ViewDataDictionary viewData, IFileProvider fileProvider, string requestPath, string sitePath, string potentialFile, string project)
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
                VDocfxMenuJson.VDocfxMenuItem menuItem = menuJsonObj.Items[i];
                menuItems[i] = new VDocFxViewModel.MenuItem(menuItem.Name, $"/{sitePath}/{menuItem.Href}", requestPath.Contains(menuItem.Href));
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
                tocItem = BuildTocItem(tocJsonObj, rel, requestPath);
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
    
    private VDocFxViewModel.TocItem BuildTocItem(VDocFxTocJson tocItem, string rel, string requestPath)
    {
        bool isActive = false;
        VDocFxViewModel.TocItem[]? children = null;

        //Go through child items first
        if (tocItem.Items != null)
        {
            children = new VDocFxViewModel.TocItem[tocItem.Items.Length];
            for (int i = 0; i < tocItem.Items.Length; i++)
            {
                VDocFxTocJson childItem = tocItem.Items[i];
                VDocFxViewModel.TocItem item = BuildTocItem(childItem, rel, requestPath);
                if (item.IsActive)
                    isActive = true;
                children[i] = item;
            }
        }
        
        string? href = tocItem.Href;
        if (href != null)
        {
            if (!href.StartsWith("/"))
                href = Path.Combine(rel, href);

            if (requestPath.Contains(href.Replace("../", "")))
                isActive = true;
        }

        VDocFxViewModel.TocItem newItem = new(tocItem.Name, href, isActive, children);
        return newItem;
    }
}