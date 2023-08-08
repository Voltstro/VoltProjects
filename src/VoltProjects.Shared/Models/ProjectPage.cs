using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A project page
/// </summary>
[Table("ProjectPage")]
public class ProjectPage
{
    /// <summary>
    ///     <see cref="ProjectPage"/> primary key.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("ProjectVersion")]
    public int ProjectVersionId { get; set; }
    public virtual ProjectVersion ProjectVersion { get; set; }
    
    /// <summary>
    ///     Is this page published? (Can it be accessed?)
    /// </summary>
    public bool Published { get; set; }
    
    /// <summary>
    ///     Page Title
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    ///     Should the title be displayed?
    /// </summary>
    public bool TitleDisplay { get; set; }
    
    /// <summary>
    ///     When was this page last modified?
    /// </summary>
    public DateTime LastModifiedTime { get; set; }
    
    /// <summary>
    ///     How many words does this page include?
    /// </summary>
    public int WordCount { get; set; }
    
    /// <summary>
    ///     Project page's TOC
    /// </summary>
    [ForeignKey("ProjectToc")]
    public int? ProjectTocId { get; set; }
    public ProjectToc? ProjectToc { get; set; }
    
    public string? TocRel { get; set; }
    
    /// <summary>
    ///     Display an aside on this page?
    /// </summary>
    public bool Aside { get; set; }
    
    /// <summary>
    ///     Page path
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     Raw HTMl Content of the page
    ///     <para>Should be using bootstrap CSS</para>
    /// </summary>
    public string Content { get; set; }
}