using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Tests.Utils;

public static class DbContextUtils
{
    /// <summary>
    ///     Creates a test <see cref="Project"/> and <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectName"></param>
    /// <param name="description"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public static ProjectVersion CreateProject(this VoltProjectDbContext dbContext, string projectName, string description, string version)
    {
        Project project = new() { Name = projectName, DisplayName = projectName, Description = description };
        dbContext.Projects.Add(project);
        ProjectVersion projectVersion = new() { Project = project, VersionTag = version, DocBuilderId = "docfx", IsDefault = true };
        dbContext.ProjectVersions.Add(projectVersion);
        
        dbContext.SaveChanges();
        return projectVersion;
    }

    /// <summary>
    ///     Creates a test <see cref="ProjectPage"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectVersion"></param>
    /// <param name="path"></param>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static ProjectPage CreateProjectPage(this VoltProjectDbContext dbContext, ProjectVersion projectVersion,
        string path, string title, string content)
    {
        string description = content.Split('.').First();
        
        ProjectPage projectPage = new ProjectPage
        {
            ProjectVersion = projectVersion,
            Path = path,
            Published = true,
            PublishedDate = DateTime.UtcNow,
            Title = title,
            TitleDisplay = false,
            Description = description,
            Content = content
        };

        dbContext.ProjectPages.Add(projectPage);
        dbContext.SaveChanges();
        return projectPage;
    }

    /// <summary>
    ///     Creates a test <see cref="ProjectToc"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectPage"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static ProjectToc CreateProjectToc(this VoltProjectDbContext dbContext, ProjectPage projectPage,
        ProjectTocItem[] items)
    {
        ProjectToc projectToc = new ProjectToc
        {
            ProjectVersion = projectPage.ProjectVersion,
            TocRel = "."
        };

        dbContext.ProjectTocs.Add(projectToc);
        dbContext.SaveChanges();

        foreach (ProjectTocItem projectTocItem in items)
        {
            //projectTocItem.ProjectToc = projectToc;
            projectTocItem.ProjectTocId = projectToc.Id;
        }
        dbContext.ProjectTocItems.AddRange(items);
        dbContext.SaveChanges();
        
        projectPage.ProjectToc = projectToc;
        projectPage.TocRel = ".";
        dbContext.SaveChanges();
        
        return projectToc;
    }

    /// <summary>
    ///     Creates a test <see cref="ProjectTocItem"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectToc"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static ProjectTocItem CreateProjectTocItem(this VoltProjectDbContext dbContext, ProjectToc projectToc,
        ProjectTocItem item)
    {
        item.ProjectToc = projectToc;
        dbContext.ProjectTocItems.Add(item);
        dbContext.SaveChanges();
        return item;
    }

    /// <summary>
    ///     Creates a test <see cref="ProjectMenuItem"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectVersion"></param>
    /// <param name="href"></param>
    /// <param name="title"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public static ProjectMenuItem CreateProjectMenuItem(this VoltProjectDbContext dbContext, ProjectVersion projectVersion,
        string href, string title, int order)
    {
        ProjectMenuItem projectMenuItem = new()
        {
            ProjectVersion = projectVersion,
            Href = href,
            Title = title,
            ItemOrder = order
        };
        dbContext.ProjectMenuItems.Add(projectMenuItem);
        dbContext.SaveChanges();
        return projectMenuItem;
    }
}