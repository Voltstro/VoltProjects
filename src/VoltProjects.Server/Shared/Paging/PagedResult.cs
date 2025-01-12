using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VoltProjects.Server.Shared.Paging;

public class PagedResult<T> : PagedCount
{
    /// <summary>
    ///     All items for this page
    /// </summary>
    public T[] Items { get; init; }

    /// <summary>
    ///     Creates and queries a paged result
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageSizes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<PagedResult<T>> Create(IQueryable<T> source, int pageIndex, int pageSize, int[]? pageSizes = null, CancellationToken cancellationToken = default)
    {
        //Make sure page index is 1 or greater
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        
        //Page size
        pageSizes ??= [15, 25, 100];

        if (!pageSizes.Contains(pageSize))
            pageSize = pageSizes[0];
        
        //Get total count
        int count = await source.CountAsync(cancellationToken);
        
        //Get paged result
        T[] items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<T>
        {
            TotalItemCount = count,
            Items = items,
            PageSize = pageSize,
            PageSizes = pageSizes,
            CurrentPage = pageIndex
        };
    }
}