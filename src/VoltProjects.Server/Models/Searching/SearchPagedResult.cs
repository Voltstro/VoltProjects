using System;

namespace VoltProjects.Server.Models.Searching;

/// <summary>
///     Result for a doc search
/// </summary>
public sealed class SearchPagedResult
{
    /// <summary>
    ///     Creates a new <see cref="SearchPagedResult"/> instance
    /// </summary>
    /// <param name="results"></param>
    /// <param name="totalResults"></param>
    /// <param name="sizePerPage"></param>
    /// <param name="page"></param>
    /// <param name="projectIds"></param>
    /// <param name="projectVersionIds"></param>
    public SearchPagedResult(SearchResult[] results, int totalResults, int sizePerPage, int page, int[] projectIds, int[] projectVersionIds)
    {
        Results = results;
        TotalResults = totalResults;
        Size = sizePerPage;
        Page = page;
        ProjectIds = projectIds;
        ProjectVersionIds = projectVersionIds;
    }
    
    /// <summary>
    ///     The total number of results
    /// </summary>
    public int TotalResults { get; init; }
    
    /// <summary>
    ///     The number of results per page
    /// </summary>
    public int Size { get; init; }
    
    /// <summary>
    ///     The current page
    /// </summary>
    public int Page { get; init; }
    
    /// <summary>
    ///     All results for this <see cref="Page"/>
    /// </summary>
    public SearchResult[] Results { get; init; }

    /// <summary>
    ///     The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((decimal)TotalResults / Size);
    
    public int[] ProjectIds { get; init; }
    public int[] ProjectVersionIds { get; init; }
}