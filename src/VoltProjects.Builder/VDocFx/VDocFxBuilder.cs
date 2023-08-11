using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.VDocFx;

/// <summary>
///     <see cref="Builder"/> for VDocFx
/// </summary>
[BuilderName(Name = "vdocfx")]
internal sealed class VDocFxBuilder : Builder
{
    private readonly ILogger<VDocFxBuilder> logger;

    public VDocFxBuilder(ILogger<VDocFxBuilder> logger)
    {
        this.logger = logger;
    }

    public override void RunBuildProcess(string docsPath, string docsBuiltPath)
    {
        string docsConfigPath = Path.Combine(docsPath, "vdocfx.yml");
        if (!File.Exists(docsConfigPath))
            throw new FileNotFoundException($"vdocfx.yml file was not found in {docsPath}!");

        ProcessStartInfo startInfo = new("vdocfx", "build --output-type PageJson")
        {
            WorkingDirectory = docsPath
        };

        Process process = new();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception("Docfx failed to exit cleanly!");
        
        process.Dispose();
    }

    public override BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath)
    {
        //Deal with menu first
        string menuFileLocation = Path.Combine(docsBuiltPath, "menu.json");
        if (!File.Exists(menuFileLocation))
            throw new FileNotFoundException("Menu.json does not exist!");
        
        VDocfxMenuJson menu = JsonSerializer.Deserialize<VDocfxMenuJson>(File.ReadAllText(menuFileLocation));
        LinkItem[] projectMenuLinkItems = new LinkItem[menu.Items.Length];
        for (int i = 0; i < projectMenuLinkItems.Length; i++)
        {
            projectMenuLinkItems[i] = new LinkItem
            {
                Title = menu.Items[i].Name,
                Href = menu.Items[i].Href
            };
        }

        //Our final project menu object
        ProjectMenu projectMenu = new ProjectMenu
        {
            LastUpdateTime = DateTime.UtcNow,
            ProjectVersionId = projectVersion.Id,
            LinkItem = new LinkItem
            {
                Items = projectMenuLinkItems
            }
        };
        
        //Build TOCs
        
        //Get all toc.json files in the project
        string[] tocFiles = Directory.GetFiles(docsBuiltPath, "toc.json", SearchOption.AllDirectories);
        ProjectToc[] tocItems = new ProjectToc[tocFiles.Length];
            
        //Convert from VDocFx to VoltProjects format
        for (int i = 0; i < tocFiles.Length; i++)
        {
            VDocFxTocJson tocModel = JsonSerializer.Deserialize<VDocFxTocJson>(File.ReadAllText(tocFiles[i]));
            string relativity = Path.GetRelativePath(docsBuiltPath, tocFiles[i]);

            tocItems[i] = new ProjectToc
            {
                TocRel = relativity,
                TocItem = BuildToc(tocModel)
            };
        }
            
        //Build pages
        string[] pageFiles = Directory.GetFiles(docsBuiltPath, "index.raw.page.json", SearchOption.AllDirectories);
        ProjectPage[] pages = new ProjectPage[pageFiles.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            string pageFile = pageFiles[i];
            VDocFxPageModel? pageModel = JsonSerializer.Deserialize<VDocFxPageModel>(File.ReadAllText(pageFile));
            if (pageModel == null)
                throw new NullReferenceException($"Failed to read page view at {pageFile}.");

            //Figure out relativity
            string relativity = Path.GetRelativePath(docsBuiltPath, pageFile.Replace("index.raw.page.json", ""));

            ProjectToc? toc = null;
            string? tocRel = null;

            bool useAside = false;
            bool useTitle = false;
            string title = "Home";
            if (pageModel.Metadata.Title != null)
            {
                useTitle = true;
                title = pageModel.Metadata.Title;
            }

            if (pageModel.Metadata.Layout == "Reference")
            {
                useTitle = false;
            }

            if (pageModel.Metadata.Layout is "Conceptual" or "Reference")
            {
                useAside = true;

                //What toc are we using?
                if (pageModel.Metadata.TocRel != null)
                {
                    string basePath = Path.GetDirectoryName(pageFile);
                    string fullTocPath = Path.Combine(basePath, pageModel.Metadata.TocRel);
                    string projectTocRel = Path.GetRelativePath(docsBuiltPath, fullTocPath);

                    toc = tocItems.FirstOrDefault(x => x.TocRel == projectTocRel);
                    if (toc != null)
                    {
                        tocRel = pageModel.Metadata.TocRel.Replace("toc.json", "");
                    }
                }
            }
            
            pages[i] = new ProjectPage
            {
                Path = relativity,
                Published = true,
                PublishedDate = DateTime.UtcNow,
                Title = title,
                TitleDisplay = useTitle,
                WordCount = pageModel.Metadata.WordCount ?? 0,
                ProjectToc = toc,
                TocRel = tocRel,
                Aside = useAside,
                Description = "Test",
                Content = pageModel.Content,
            };
        }

        return new BuildResult(projectMenu, tocItems, pages);
    }

    private LinkItem BuildToc(VDocFxTocJson tocModel)
    {
        LinkItem[] childTocItems = new LinkItem[tocModel.Items?.Length ?? 0];
        for (int i = 0; i < childTocItems.Length; i++)
        {
            VDocFxTocJson childModel = tocModel.Items![i];
            childTocItems[i] = BuildToc(childModel);
        }
        
        LinkItem newToc = new()
        {
            Href = tocModel.Href,
            Title = tocModel.Name,
            Items = childTocItems.Length > 0 ? childTocItems : null
        };

        return newToc;
    }
}
