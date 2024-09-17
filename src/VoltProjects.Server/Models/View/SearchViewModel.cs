using VoltProjects.Server.Models.Searching;
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
    /// <param name="projects"></param>
    /// <param name="query"></param>
    /// <param name="searchResult"></param>
    public SearchViewModel(ProjectSearch[] projects, string? query, SearchPagedResult? searchResult)
    {
        Projects = projects;
        Query = query;
        PagedResult = searchResult;
    }
    
    /// <summary>
    ///     All available projects
    /// </summary>
    public ProjectSearch[] Projects { get; init; }
    
    /// <summary>
    ///     Inputted search query
    /// </summary>
    public string? Query { get; init; }
    
    /// <summary>
    ///     <see cref="SearchPagedResult"/> result for this <see cref="Query"/>
    /// </summary>
    public SearchPagedResult? PagedResult { get; init; }
}