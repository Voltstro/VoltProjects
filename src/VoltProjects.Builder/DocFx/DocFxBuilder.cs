using System.Text.Json;
using HtmlAgilityPack;
using VoltProjects.Builder.Core;
using VoltProjects.Builder.VDocFx;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.DocFx;

[BuilderName(Name = "docfx")]
public class DocFxBuilder : Core.Builder
{
    public override void PrepareBuilder(ref string[]? arguments, string docsPath, string docsBuiltPath)
    {
        string docsConfigPath = Path.Combine(docsPath, "docfx.json");
        if (!File.Exists(docsConfigPath))
            throw new FileNotFoundException($"docfx.json file was not found in {docsPath}!");

        //Format arguments (if provided)
        if (arguments != null)
        {
            string sitePath = Path.Combine(docsPath, "_site");

            /*
            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] = string.Format(arguments[i], sitePath);
            }
            */
        }
    }

    public override BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath)
    {
        //Get all models first
        string[] modelFiles = Directory.GetFiles(docsBuiltPath, "*.raw.json", SearchOption.AllDirectories);

        //Deal with regular TOCs first
        List<ProjectToc> projectTocs = new();
        string menuTocLocation = Path.Combine(docsBuiltPath, "toc.raw.json");
        foreach (string modelFilePath in modelFiles.Where(x => Path.GetFileName(x) == "toc.raw.json" && x != menuTocLocation))
        {
            string pathRelativity = Path.GetRelativePath(docsBuiltPath, modelFilePath);
            DocFxRawModel model = JsonSerializer.Deserialize<DocFxRawModel>(File.ReadAllText(modelFilePath));

            List<LinkItem> linkItems = new List<LinkItem>();
            for (int i = 0; i < model.Items.Length; i++)
            {
                DocFxRawModel.DocfxMenuItem menuItem = model.Items[i];
                LinkItem? buildToc = BuildToc(menuItem);
                if(buildToc != null)
                    linkItems.Add(buildToc);
            }
                
            ProjectToc projectToc = new()
            {
                ProjectVersionId = projectVersion.Id,
                LastUpdateTime = DateTime.UtcNow,
                TocRel = pathRelativity,
                TocItem = new LinkItem
                {
                    Items = linkItems.ToArray()
                }
            };
            projectTocs.Add(projectToc);
        }
        
        //Do main project menu, it should be located at root
        string? projectMenuPath = modelFiles.FirstOrDefault(x => x == menuTocLocation);
        if (projectMenuPath == null)
            throw new NullReferenceException("Root project TOC could not be found!");
        
        DocFxRawModel projectMenuModel = JsonSerializer.Deserialize<DocFxRawModel>(File.ReadAllText(projectMenuPath));
        LinkItem[] projectMenuLinks = new LinkItem[projectMenuModel.Items.Length];

        for (int i = 0; i < projectMenuModel.Items.Length; i++)
        {
            DocFxRawModel.DocfxMenuItem tocItem = projectMenuModel.Items[i];
            string? href = tocItem.Href;

            if (href != null && href.EndsWith("index.html"))
                href = href[..^10];

            if (href != null && href.EndsWith(".html"))
                href = $"{href[..^5]}/";
                    
            projectMenuLinks[i] = new LinkItem
            {
                Title = projectMenuModel.Items[i].Name,
                Href = href
            };
        }

        ProjectMenu projectMenu = new()
        {
            LastUpdateTime = DateTime.UtcNow,
            ProjectVersionId = projectVersion.Id,
            LinkItem = new LinkItem
            {
                Items = projectMenuLinks
            }
        };
        
        //Now for project pages
        List<ProjectPage> projectPages = new();
        foreach (string modelFilePath in modelFiles.Where(x => !x.Contains(Path.GetFileName("toc.raw.json"))))
        {
            DocFxRawModel model = JsonSerializer.Deserialize<DocFxRawModel>(File.ReadAllText(modelFilePath));
            
            //Only deal with "Conceptual" right now
            if(model.Type != "Conceptual" && model.Uid == null)
                continue;

            string? pageTitle = model.Title;
            
            string pathRelativity = Path.GetRelativePath(docsBuiltPath, modelFilePath.Replace(".raw.json", ""));

            //Hide 'index' paths
            if (pathRelativity.EndsWith("index"))
                pathRelativity = pathRelativity[..^5];

            //Paths should be full
            if (!pathRelativity.EndsWith("/"))
                pathRelativity = $"{pathRelativity}/";

            //Root file becomes '.'
            if (pathRelativity == "/")
                pathRelativity = ".";
            
            //Figure out TOC rel
            string? tocPath = model.TocPath;
            ProjectToc? projectToc = null;
            if (tocPath != null)
            {
                tocPath = tocPath.Replace("toc.html", "toc.raw.json");
                projectToc = projectTocs.FirstOrDefault(x => x.TocRel == tocPath);
                if (projectToc != null)
                {
                    string tocDirectory = model.TocPath.Replace("toc.html", "");
                    string tocRel = Path.GetRelativePath(pathRelativity, tocDirectory);
                    tocPath = tocRel == "." ? string.Empty : $"{tocRel}/";
                }
                else
                    tocPath = null;
            }
            
            string htmlFilePath = Path.Combine(docsBuiltPath, model.Path);
            HtmlDocument doc = new();
            doc.LoadHtml(File.ReadAllText(htmlFilePath));

            HtmlNode? articleNode = doc.DocumentNode.SelectSingleNode("//article");
            if (articleNode == null)
                throw new NullReferenceException("Could not find article node!");

            //Remove title node, VP does it
            HtmlNode? firstNode = articleNode.ChildNodes[1];
            if (firstNode is { Name: "h1" })
                articleNode.ChildNodes.Remove(firstNode);

            if (pageTitle == null)
                pageTitle = firstNode.InnerText.Replace("\n", "").TrimStart().TrimEnd();
            
            //Process links, DocFx uses ugly links, we do not
            HtmlNodeCollection? links = articleNode.SelectNodes("//a/@href");
            if (links != null)
            {
                foreach (HtmlNode htmlNode in links)
                {
                    HtmlAttribute? href = htmlNode.Attributes.FirstOrDefault(x => x.Name == "href");
                    if (href != null )
                    {
                        string hrefValue = href.Value;
                        if(hrefValue.StartsWith("http")) continue;

                        string[] splitLink = hrefValue.Split("#");
                        href.Value = splitLink.Length == 2 ? $"../{splitLink[0][..^5]}/#{splitLink[1]}" : $"../{splitLink[0][..^5]}/";
                    }
                }
            }
            

            string pageContent = articleNode.InnerHtml;
            
            projectPages.Add(new ProjectPage
            {
                Path = pathRelativity,
                Published = true,
                PublishedDate = DateTime.UtcNow,
                Title = pageTitle,
                TitleDisplay = true,
                WordCount = model.WordCount,
                ProjectToc = projectToc,
                TocRel = tocPath,
                Aside = true,
                Content = pageContent
            });
            
            Console.WriteLine(pathRelativity);
        }
        
        return new BuildResult(projectMenu, projectTocs.ToArray(), projectPages.ToArray());
    }
    
    private LinkItem? BuildToc(DocFxRawModel.DocfxMenuItem tocModel)
    {
        List<LinkItem> childTocItems = new List<LinkItem>();
        if (tocModel.Items != null)
        {
            for (int i = 0; i < tocModel.Items.Length; i++)
            {
                DocFxRawModel.DocfxMenuItem childModel = tocModel.Items![i];
                LinkItem? builtChildToc = BuildToc(childModel);
                if(builtChildToc != null)
                    childTocItems.Add(builtChildToc);
            }
        }
        
        string? href = tocModel.Href;
        if (href != null && href.EndsWith("index.html"))
            return null;
        
        if (href != null && href.EndsWith(".html"))
            href = $"{href[..^5]}/";
        
        LinkItem newToc = new()
        {
            Href = href,
            Title = tocModel.Name,
            Items = childTocItems.Count > 0 ? childTocItems.ToArray() : null
        };

        return newToc;
    }
}