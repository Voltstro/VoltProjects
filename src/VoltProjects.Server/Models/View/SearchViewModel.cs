using VoltProjects.Server.Models.Searching;
using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

/// <summary>
///     View model for the search page
/// </summary>
public sealed class SearchViewModel
{
    /// <summary>
    ///     Creates a new <see cref="SearchViewModel"/> instance
    /// </summary>
    /// <param name="query"></param>
    /// <param name="projects"></param>
    /// <param name="selectedProject"></param>
    public SearchViewModel(string? query, ProjectVersion[] projects, ProjectVersion? selectedProject, PagedResult<SearchResult>? searchResult)
    {
        Query = query;
        Projects = projects;
        Project = selectedProject;
        SearchResult = searchResult;
    }
    
    /// <summary>
    ///     All projects available
    /// </summary>
    public ProjectVersion[] Projects { get; }

    /// <summary>
    ///     Selected project
    /// </summary>
    public ProjectVersion? Project { get; }
    
    /// <summary>
    ///     Inputted search query
    /// </summary>
    public string? Query { get; }
    
    public PagedResult<SearchResult>? SearchResult { get; }
}