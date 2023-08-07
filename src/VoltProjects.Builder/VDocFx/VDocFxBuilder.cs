using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoltProjects.Builder.Core;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;
using WebMarkupMin.Core;

namespace VoltProjects.Builder.VDocFx;

/// <summary>
///     <see cref="Builder"/> for VDocFx
/// </summary>
[BuilderNameAttribute(Name = "VDocFx")]
internal sealed class VDocFxBuilder : Builder
{
    private readonly ILogger<VDocFxBuilder> logger;
    private readonly HtmlMinifier htmlMinifier;

    public VDocFxBuilder(ILogger<VDocFxBuilder> logger, HtmlMinifier htmlMinifier)
    {
        this.logger = logger;
        this.htmlMinifier = htmlMinifier;
    }
    
    public override void BuildProject(ProjectVersion projectVersion, VoltProjectDbContext dbContext)
    {
        string projectPath = projectVersion.Project.GitUrl;
        bool isGitUrl = projectPath.StartsWith("https://");
        if (isGitUrl)
        {
            //TODO: Support git URLS
            throw new NotImplementedException();
        }

        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException("Project directory not found!");
        
        string docsLocation = Path.Combine(projectPath, projectVersion.DocsPath);
        if (!Directory.Exists(docsLocation))
            throw new DirectoryNotFoundException("Failed to find docs dir!");

        RunDocFxProcess(docsLocation);
        
        //Check built docs exist
        string builtDocsLocation = Path.Combine(projectPath, projectVersion.DocsBuiltPath);
        if (!Directory.Exists(builtDocsLocation))
            throw new DirectoryNotFoundException("Failed to find built docs dir!");
        
        //Process menu first
        {
            string menuFileLocation = Path.Combine(builtDocsLocation, "menu.json");
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

            ProjectMenu projectMenu = new ProjectMenu
            {
                LastUpdateTime = DateTime.UtcNow,
                ProjectVersionId = projectVersion.Id,
                LinkItem = new LinkItem
                {
                    Items = projectMenuLinkItems
                }
            };
            
            //Upsert project menu
            string json = JsonSerializer.Serialize(projectMenu.LinkItem);
            dbContext.Database
                .ExecuteSql(
                    $"INSERT INTO public.\"ProjectMenu\" (\"ProjectVersionId\", \"LastUpdateTime\", \"LinkItem\") VALUES ({projectMenu.ProjectVersionId}, {projectMenu.LastUpdateTime}, {json}::jsonb) ON CONFLICT (\"ProjectVersionId\") DO UPDATE SET \"LastUpdateTime\" = EXCLUDED.\"LastUpdateTime\", \"LinkItem\" = EXCLUDED.\"LinkItem\" RETURNING *;");
        }
        
        //Build toc items
        string[] tocFiles = Directory.GetFiles(builtDocsLocation, "toc.json", SearchOption.AllDirectories);
        ProjectToc[] allTocs;
        {
            TocItem[] tocItems = new TocItem[tocFiles.Length];
            
            for (int i = 0; i < tocFiles.Length; i++)
            {
                VDocFxTocJson tocModel = JsonSerializer.Deserialize<VDocFxTocJson>(File.ReadAllText(tocFiles[i]));
                string relativity = Path.GetRelativePath(builtDocsLocation, tocFiles[i]);

                tocItems[i] = new TocItem
                {
                    TocRel = relativity,
                    TocLinkItem = BuildToc(tocModel)
                };
            }
            
            int tocIndex = 1;
            object[] tocParams = new object[1 + tocItems.Length * 2];
            tocParams[0] = projectVersion.Id;
            string tocParamsPlaceholder = string.Join(",", tocItems.Select(x =>
            {
                tocParams[tocIndex] = x.TocRel;
                tocParams[tocIndex + 1] = JsonSerializer.Serialize(x.TocLinkItem);
               
                return $"ROW(@p{tocIndex++}, @p{tocIndex++}::jsonb)";
            }));
           
           allTocs = dbContext.ProjectTocs.FromSqlRaw($"SELECT * FROM public.\"UpsertProjectTOCs\"(@p0, ARRAY[{tocParamsPlaceholder}]::upsertedtoc[]);", tocParams).ToArray();
        }
        
        //Build project pages
        string[] pageFiles = Directory.GetFiles(builtDocsLocation, "index.raw.page.json", SearchOption.AllDirectories);
        ProjectPage[] pages = new ProjectPage[pageFiles.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            string pageFile = pageFiles[i];
            VDocFxPageModel? pageModel = JsonSerializer.Deserialize<VDocFxPageModel>(File.ReadAllText(pageFile));
            if (pageModel == null)
                throw new NullReferenceException($"Failed to read page view at {pageFile}.");
            
            //Figure out relativity
            string relativity = Path.GetRelativePath(builtDocsLocation, pageFile.Replace("index.raw.page.json", ""));

            ProjectToc? toc = null;
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
                    string tocRel = Path.GetRelativePath(builtDocsLocation, fullTocPath);

                    toc = allTocs.FirstOrDefault(x => x.TocRel == tocRel);
                }
            }
            
            //Minify HTML
            MarkupMinificationResult? minifyResult = htmlMinifier.Minify(pageModel.Content);
            
            pages[i] = new ProjectPage
            {
                ProjectVersionId = projectVersion.Id,
                Published = true,
                Title = title,
                TitleDisplay = useTitle,
                LastModifiedTime = DateTime.UtcNow,
                WordCount = pageModel.Metadata.WordCount ?? 0,
                Aside = useAside,
                Path = relativity,
                Content = minifyResult.MinifiedContent,
                ProjectTocId = toc?.Id
            };
        }

        int pageIndex = 1;
        object?[] pageParams = new object[1 + pages.Length * 7];
        pageParams[0] = projectVersion.Id;
        var pageParamsPlaceholder = string.Join(",", pages.Select(x =>
        {
            pageParams[pageIndex] = x.Title;
            pageParams[pageIndex + 1] = x.TitleDisplay;
            pageParams[pageIndex + 2] = x.Aside;
            pageParams[pageIndex + 3] = x.WordCount;
            pageParams[pageIndex + 4] = x.ProjectTocId;
            pageParams[pageIndex + 5] = x.Path;
            pageParams[pageIndex + 6] = x.Content;
               
            return $"ROW(@p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++}, @p{pageIndex++})";
        }));

        //Upsert project pages
        //No return needed on this one, so we will use dbContext.Database
        dbContext.Database.ExecuteSqlRaw(
            $"SELECT public.\"UpsertProjectPages\"(@p0, ARRAY[{pageParamsPlaceholder}]::upsertedpage[]);", pageParams);
        
        dbContext.SaveChanges();
        logger.LogInformation("Built doc");
    }

    private void RunDocFxProcess(string docsPath)
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
