using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Server.Models.Searching;
using VoltProjects.Server.Shared.Paging;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;


namespace VoltProjects.Server.Services;

/// <summary>
///		Backend service for searching
/// </summary>
public sealed class SearchService
{
	private readonly VoltProjectDbContext dbContext;
    private readonly HtmlSanitizer sanitizer;

    /// <summary>
    ///		The main search SQL query
    /// </summary>
    private const string MainQuerySql = @"
SELECT
	ts_headline(language_configuration, title || content, websearch_to_tsquery(@p0), 'StartSel=<mark>,StopSel=</mark>') AS headline,
	project_page.path AS path,
	project_page.title AS title,
	project.name AS project_name,
	project.display_name AS project_display_name,
	project_version.version_tag AS project_version
FROM public.project_page AS project_page
JOIN public.project_version AS project_version
	ON project_version.id = project_page.project_version_id 
JOIN public.project AS project
	ON project.id = project_version.project_id
WHERE
	to_tsvector(language_configuration, title || content) @@ websearch_to_tsquery(@p0)
AND project_page.published = TRUE
AND project.id = @p1
AND project_version.id = @p2
ORDER BY (
	ts_rank_cd(to_tsvector(language_configuration, title || content), websearch_to_tsquery(@p0))
) DESC
";
    
    /// <summary>
    ///		Creates a new <see cref="SearchService"/> instance
    /// </summary>
    public SearchService(VoltProjectDbContext dbContext)
    {
	    this.dbContext = dbContext;
        sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions
        {
	        AllowedTags = new HashSet<string>
	        {
		        "mark"
	        }
        });
    }

    /// <summary>
    ///		Performs a search query
    /// </summary>
    /// <param name="query"></param>
    /// <param name="projectId"></param>
    /// <param name="projectVersionId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<PagedResult<SearchResult>> Search(string query, int projectId, int projectVersionId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
	    IQueryable<SearchResult> searchQuery = dbContext.Database.SqlQueryRaw<SearchResult>(MainQuerySql, query, projectId, projectVersionId);
	    PagedResult<SearchResult> pagedResult = await PagedResult<SearchResult>.Create(searchQuery, pageIndex, pageSize, [5, 15, 25, 50], [15, 25, 50],
		    items =>
		    {
			    foreach (SearchResult result in items)
			    {
				    result.Headline = sanitizer.Sanitize(result.Headline);
			    }
		    }, cancellationToken);
	    return pagedResult;
    }
}