using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A project page
/// </summary>
[Table("project_page")]
public class ProjectPage
{
    /// <summary>
    ///     <see cref="ProjectPage"/> primary key.
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }

    /// <summary>
    ///     Page path
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     Is this page published? (Can it be accessed?)
    /// </summary>
    public bool Published { get; set; }
    
    /// <summary>
    ///     The date to display as published
    /// </summary>
    public DateTime PublishedDate { get; set; }
    
    /// <summary>
    ///     The ID of the parent page
    /// </summary>
    [ForeignKey("ParentPage")]
    public int? ParentPageId { get; set; }
    
    /// <summary>
    ///     The parent page
    /// </summary>
    public ProjectPage? ParentPage { get; set; }

    /// <summary>
    ///     Page Title
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    ///     Should the title be displayed?
    /// </summary>
    public bool TitleDisplay { get; set; }
    
    /// <summary>
    ///     How many words does this page include?
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    ///     Project page's TOC
    /// </summary>
    [ForeignKey("ProjectToc")]
    public int? ProjectTocId { get; set; }
    public ProjectToc? ProjectToc { get; set; }
    
    /// <summary>
    ///     What is the toc relativity for this page?
    /// </summary>
    public string? TocRel { get; set; }
    
    /// <summary>
    ///     Original git URL
    /// </summary>
    public string? GitUrl { get; set; }
    
    /// <summary>
    ///     Display an aside on this page?
    /// </summary>
    public bool Aside { get; set; }
    
    /// <summary>
    ///     Page's description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Raw HTMl Content of the page
    ///     <para>Should be using bootstrap CSS</para>
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    ///     When was this page last updated?
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    ///     When was this page created?
    /// </summary>
    public DateTime CreationTime { get; set; }
}