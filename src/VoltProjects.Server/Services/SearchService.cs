using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.Searching;
using VoltProjects.Shared;

namespace VoltProjects.Server.Services;

/// <summary>
///		Backend service for searching
/// </summary>
public sealed class SearchService
{
	/// <summary>
	///		Number of returned results per page
	/// </summary>
	public const int DefaultResultsPerPage = 10;
	
    private readonly HtmlSanitizer sanitizer;
    
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
	    //Parameters for query
	    NpgsqlParameter querySql = new("querySql", query);
	    NpgsqlParameter projectIdsSql = new("projectIdsSql", projectIds);
	    NpgsqlParameter projectVersionIdsSq = new("projectVersionIdsSq", projectVersionIds);
	    
        IQueryable<SearchResult> sqlQuery = dbContext.Database.SqlQuery<SearchResult>($@"
SELECT
	ts_headline(language_configuration, content, websearch_to_tsquery({querySql}), 'StartSel=<mark>,StopSel=</mark>') AS headline,
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
	to_tsvector(language_configuration, content) @@ websearch_to_tsquery({querySql})
AND project_page.published = TRUE
AND project.id = ANY({projectIdsSql})
AND project_version.id = ANY({projectVersionIdsSq})
ORDER BY (
	ts_rank_cd(to_tsvector(language_configuration, content), websearch_to_tsquery({querySql}))
) DESC
");
        //Get potential total number of returned results
        int totalResults = sqlQuery.Count();
        
        //Get paged results
        SearchResult[] results = await sqlQuery
	        .Skip((page - 1) * size)
	        .Take(size)
	        .ToArrayAsync(cancellationToken);
        
        //Sanitize results, excluding <mark>
        foreach (SearchResult result in results)
        {
	        result.Headline = sanitizer.Sanitize(result.Headline);
        }

        return new SearchPagedResult(results, totalResults, size, page);
    }
}