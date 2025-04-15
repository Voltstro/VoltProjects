using System;

namespace VoltProjects.Server.Shared.Paging;

public abstract class PagedCount
{
    /// <summary>
    ///     How many potential items total
    /// </summary>
    public int TotalItemCount { get; init; }
    
    /// <summary>
    ///     Items per page
    /// </summary>
    public int PageSize { get; init; }
    
    /// <summary>
    ///     Available page sizes
    /// </summary>
    public int[] PageSizes { get; init; }
    
    /// <summary>
    ///     Current page
    /// </summary>
    public int CurrentPage { get; init; }
    
    /// <summary>
    ///     The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItemCount / PageSize);
}