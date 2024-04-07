using System.Text.Json;
using HtmlAgilityPack;
using VoltProjects.Builder.Core;
using VoltProjects.Shared.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoltProjects.Builder.Builders.MkDocs;

[BuilderName(Name = "mkdocs")]
public class MkDocsBuilder : Core.Builder
{
    private readonly IDeserializer ymlDeserializer;
    private readonly ISerializer ymlSerializer;
    
    public MkDocsBuilder()
    {
        ymlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        
        ymlSerializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }
    
    private readonly Dictionary<string, string> admonitionCssMapping = new()
    {
        ["admonition"] = "alert",
        
        ["note"] = "alert-info",
        ["abstract"] = "alert-info",
        ["info"] = "alert-info",
        
        ["tip"] = "alert-success",
        ["success"] = "alert-success",
        
        ["warning"] = "alert-warning",
        
        ["failure"] = "alert-danger",
        ["danger"] = "alert-danger",
        ["bug"] = "alert-danger",
        
        ["example"] = "alert-primary",
        
        ["quote"] = "alert-light",
    };
    
    public override void PrepareBuilder(ref string[]? arguments, string docsPath, string docsBuiltPath)
    {
        string configFile = Path.Combine(docsPath, "mkdocs.yml");
        if (!File.Exists(configFile))
            throw new FileNotFoundException("Config file mkdocs.yml was not found!");

        //Load as a dictionary, not the best idea, but better then having to create class
        //with all of MkDoc config options
        Dictionary<string, object> config = ymlDeserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(configFile));
        bool configContainsPlugins = config.ContainsKey("plugins");
        if (!configContainsPlugins)
        {
            string[] plugins = ["vp_integration"];
            config.Add("plugins", plugins);
        }
        else
        {
            IList<object> plugins = (config["plugins"] as IList<object>)!;
            if(!plugins.Contains("vp_integration"))
                plugins.Add("vp_integration");
        }

        string ymlConfig = ymlSerializer.Serialize(config);
        File.WriteAllText(configFile, ymlConfig);
    }

    public override BuildResult BuildProject(ProjectVersion projectVersion, string docsPath, string docsBuiltPath)
    {
        string menuFile = Path.Combine(docsBuiltPath, "menu.json");
        string tocFile = Path.Combine(docsBuiltPath, "tocs.json");
        string pagesFile = Path.Combine(docsBuiltPath, "pages.json");

        if (!File.Exists(menuFile) || !File.Exists(tocFile) || !File.Exists(pagesFile))
            throw new FileNotFoundException("Either menu.json, tocs.json or pages.json file(s) are missing!");

        //Create project menu
        LinkItem? menuItem = JsonSerializer.Deserialize<LinkItem>(File.ReadAllText(menuFile));
        if (menuItem == null)
            throw new NullReferenceException("Menu.json was deserialized as null!");

        ProjectMenu projectMenu = new()
        {
            ProjectVersionId = projectVersion.Id,
            LinkItem = menuItem
        };
        
        //Create TOCs
        MkDocsVpIntegrationModels.TocModel tocsModel = JsonSerializer.Deserialize<MkDocsVpIntegrationModels.TocModel>(File.ReadAllText(tocFile));
        ProjectToc[] projectTocs = new ProjectToc[tocsModel.Tocs.Length];
        for (int i = 0; i < projectTocs.Length; i++)
        {
            projectTocs[i] = new ProjectToc
            {
                ProjectVersionId = projectVersion.Id,
                TocRel = tocsModel.Tocs[i].TocIndex,
                TocItem = tocsModel.Tocs[i].LinkItem
            };
        }
        
        //Create pages
        MkDocsVpIntegrationModels.PagesModel mkPages = JsonSerializer.Deserialize<MkDocsVpIntegrationModels.PagesModel>(File.ReadAllText(pagesFile));
        ProjectPage[] pages = new ProjectPage[mkPages.Pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            MkDocsVpIntegrationModels.Page mkDocsPage = mkPages.Pages[i];

            string pagePath = mkDocsPage.Path;
            if (string.IsNullOrWhiteSpace(pagePath))
                pagePath = ".";

            //Calculate github file path
            string githubPath = Path.Combine(projectVersion.Project.GitUrl, "blob", projectVersion.GitBranch,
                projectVersion.DocsPath, mkDocsPage.FilePath);

            //Calculate TOC rel. All paths should be from root
            string fullPagePathSystem = Path.GetFullPath(Path.Combine(docsPath, pagePath));
            string? tocRel = mkDocsPage.TocIndex != null ? Path.GetRelativePath(fullPagePathSystem, docsPath) : null;
            
            HtmlDocument doc = new();
            doc.LoadHtml(mkDocsPage.Content);

            //Fix up tables
            HtmlNodeCollection? tables = doc.DocumentNode.SelectNodes(".//table");
            if (tables != null)
            {
                foreach (HtmlNode tableNode in tables)
                {
                    if(tableNode.Attributes.FirstOrDefault(x => x.Name == "class") != null)
                        continue;

                    tableNode.SetAttributeValue("class", "table");
                }
            }
            
            //Fix up admonition
            HtmlNodeCollection? admonitions = doc.DocumentNode.SelectNodes(".//div[contains(@class, 'admonition')]");
            if (admonitions != null)
            {
                foreach (HtmlNode admonition in admonitions)
                {
                    HtmlAttribute classAttribute = admonition.Attributes["class"];
                    string[] classes = classAttribute.Value.Split(" ");
                    for (int x = 0; x < classes.Length; x++)
                    {
                        if (admonitionCssMapping.ContainsKey(classes[x]))
                            classes[x] = admonitionCssMapping[classes[x]];
                    }

                    classAttribute.Value = string.Join(" ", classes);

                    HtmlNode? titleNode = admonition.SelectSingleNode("//p[contains(@class, 'admonition-title')]");
                    if (titleNode == null)
                        continue;
                
                    titleNode.Name = "h5";
                    titleNode.Attributes.RemoveAll();
                }
            }
            
            //Remove header (if it is the first one)
            bool displayTitle = true;
            bool displayMetabar = true;
            HtmlNodeCollection? childNodes = doc.DocumentNode.ChildNodes;
            if (childNodes != null)
            {
                HtmlNode? firstNode = childNodes.First();
                if(firstNode.Name == "h1")
                    firstNode.Remove();
                else
                {
                    displayTitle = false;
                    displayMetabar = false;
                }
            }
            
            string pageContent = doc.DocumentNode.InnerHtml;
            
            pages[i] = new ProjectPage
            {
                ProjectVersionId = projectVersion.Id,
                Path = pagePath,
                Content = pageContent,
                Aside = true,
                Metabar = displayMetabar,
                Title = mkDocsPage.Title,
                TitleDisplay = displayTitle,
                GitUrl = githubPath,
                ProjectToc = mkDocsPage.TocIndex != null ? projectTocs.FirstOrDefault(x => x.TocRel == mkDocsPage.TocIndex) : null,
                TocRel = tocRel,
                PublishedDate = DateTime.UtcNow
            };
        }

        return new BuildResult(projectMenu, projectTocs, pages);
    }
}