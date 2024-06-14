using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Server.Models.Searching;
using VoltProjects.Shared;

namespace VoltProjects.Server.Services;

/// <summary>
///		Backend service for searching
/// </summary>
public sealed class SearchService
{
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
AND project.id = ANY(@p1)
AND project_version.id = ANY(@p2)
ORDER BY (
	ts_rank_cd(to_tsvector(language_configuration, title || content), websearch_to_tsquery(@p0))
) DESC
";

    private const string CountQuerySql = @"
SELECT
	count(*) as ""Value""
FROM public.project_page AS project_page
JOIN public.project_version AS project_version
	ON project_version.id = project_page.project_version_id 
JOIN public.project AS project
	ON project.id = project_version.project_id
WHERE
	to_tsvector(language_configuration, title || content) @@ websearch_to_tsquery(@p0)
AND project_page.published = TRUE
AND project.id = ANY(@p1)
AND project_version.id = ANY(@p2)
";
    
    /// <summary>
    ///		Creates a new <see cref="SearchService"/> instance
    /// </summary>
    public SearchService()
    {
        sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions
        {
	        AllowedTags = new HashSet<string>
	        {
		        "mark"
	        }
        });
    }

    public async Task<SearchPagedResult> Search(VoltProjectDbContext dbContext, string query, int page, int size, int[] projectIds, int[] projectVersionIds, CancellationToken cancellationToken = default)
    {
	    Stopwatch stopwatch = Stopwatch.StartNew();
	    
	    //Get total results. Yes, doing ToArray()[0] is not pretty, but it works
	    IQueryable<int> countSearchSqlQuery =
		    dbContext.Database.SqlQueryRaw<int>(CountQuerySql, query, projectIds, projectVersionIds);
	    int totalResults = countSearchSqlQuery.ToArray()[0];
	    
	    //Get actual paged results
	    IQueryable<SearchResult> mainSearchSqlQuery =
		    dbContext.Database.SqlQueryRaw<SearchResult>(MainQuerySql, query, projectIds, projectVersionIds);
        SearchResult[] results = await mainSearchSqlQuery
	        .Skip((page - 1) * size)
	        .Take(size)
	        .ToArrayAsync(cancellationToken);
        
        //Sanitize results, excluding <mark>
        foreach (SearchResult result in results)
        {
	        result.Headline = sanitizer.Sanitize(result.Headline);
        }
        
        stopwatch.Stop();
        return new SearchPagedResult(results, totalResults, size, page, stopwatch.Elapsed.TotalSeconds, projectIds, projectVersionIds);
    }
}