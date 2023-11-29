using System.Text.Json;
using VoltProjects.Builder.Core;
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

        ProjectMenu? projectMenu = null;
        List<ProjectPage> projectPages = new List<ProjectPage>();

        string menuTocLocation = Path.Combine(docsBuiltPath, "toc.raw.json");
        foreach (string modelFilePath in modelFiles)
        {
            DocFxRawModel model = JsonSerializer.Deserialize<DocFxRawModel>(File.ReadAllText(modelFilePath));
            
            //If this is root toc, then treat it like the root meuu
            if (modelFilePath == menuTocLocation)
            {
                LinkItem[] links = new LinkItem[model.Items.Length];

                for (int i = 0; i < model.Items.Length; i++)
                {
                    DocFxRawModel.DocfxMenuItem tocItem = model.Items[i];
                    string href = tocItem.Href;
                    if (href.EndsWith(".html"))
                        href = $"{href[..^5]}/";
                    
                    links[i] = new LinkItem
                    {
                        Title = model.Items[i].Name,
                        Href = href
                    };
                }

                projectMenu = new ProjectMenu
                {
                    LastUpdateTime = DateTime.UtcNow,
                    ProjectVersionId = projectVersion.Id,
                    LinkItem = new LinkItem
                    {
                        Items = links
                    }
                };
                continue;
            }
            
            if(model.Type != "Conceptual")
                continue;
            
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
            
            projectPages.Add(new ProjectPage
            {
                Path = pathRelativity,
                Published = true,
                PublishedDate = DateTime.UtcNow,
                Title = model.Title,
                TitleDisplay = true,
                WordCount = model.WordCount,
                ProjectToc = null,
                TocRel = null,
                Aside = true,
                Content = model.Conceptual,
            });
            
            Console.WriteLine(pathRelativity);
            
            
        }
        
        return new BuildResult(projectMenu, Array.Empty<ProjectToc>(), projectPages.ToArray());
    }
}