using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

/// <summary>
///		Contains methods for <see cref="VoltProjectDbContext"/> that are more complex and require raw SQL
/// </summary>
[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
public static class DbProcedures
{
	/// <summary>
	///		Upserts <see cref="ProjectToc"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="projectTocs"></param>
	/// <returns></returns>
    public static async Task<ProjectToc[]> UpsertProjectTocs(this VoltProjectDbContext dbContext, ProjectToc[] projectTocs)
    {
	    (object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectTocs, x => new { x.ProjectVersionId, x.TocRel }, false);

	    string paramsPlaceholder = string.Join(",", objectPlaceholders);
	    
	    IQueryable<ProjectToc> results = dbContext.ProjectTocs.FromSqlRaw($"""
	                                                                       MERGE INTO public.project_toc AS pt
	                                                                       	USING(SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_version_id, toc_rel)) AS toc_values
	                                                                       	ON toc_values.project_version_id = pt.project_version_id AND toc_values.toc_rel = pt.toc_rel
	                                                                       WHEN NOT MATCHED THEN
	                                                                       	INSERT (project_version_id, toc_rel)
	                                                                       	VALUES (toc_values.project_version_id, toc_values.toc_rel)
	                                                                       WHEN MATCHED THEN
	                                                                       	UPDATE SET 
	                                                                       		toc_rel = toc_values.toc_rel
	                                                                       RETURNING pt.*;
	                                                                       """, objectValues);

	    return await results.ToArrayAsync();
    }

	/// <summary>
	///		Upserts <see cref="ProjectTocItem"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="projectTocItems"></param>
	/// <returns></returns>
	public static async Task<ProjectTocItem[]> UpsertProjectTocItems(this VoltProjectDbContext dbContext,
		ProjectTocItem[] projectTocItems)
	{
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectTocItems, x => new { x.ProjectTocId, x.ProjectVersionId, x.Title, x.ItemOrder, x.ParentTocItemId, x.Href }, false);

		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		
		IQueryable<ProjectTocItem> results = dbContext.ProjectTocItems.FromSqlRaw($"""
		                                                                           MERGE INTO public.project_toc_item AS pti
		                                                                           USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_toc_id, project_version_id, title, item_order, parent_toc_item_id, href)) AS toc_items_values
		                                                                           	ON toc_items_values.project_toc_id = pti.project_toc_id
		                                                                           	AND toc_items_values.project_version_id = pti.project_version_id
		                                                                            AND toc_items_values.title = pti.title
		                                                                            AND pti.parent_toc_item_id IS NOT DISTINCT FROM toc_items_values.parent_toc_item_id::int
		                                                                            AND pti.href IS NOT DISTINCT FROM toc_items_values.href
		                                                                           WHEN NOT MATCHED THEN
		                                                                           	INSERT (project_toc_id, project_version_id, title, item_order, parent_toc_item_id, href)
		                                                                           	VALUES (toc_items_values.project_toc_id, toc_items_values.project_version_id, toc_items_values.title, toc_items_values.item_order, toc_items_values.parent_toc_item_id::int, toc_items_values.href)
		                                                                           WHEN MATCHED THEN
		                                                                           	UPDATE SET 
		                                                                           		title = toc_items_values.title,
		                                                                           		item_order = toc_items_values.item_order
		                                                                           RETURNING pti.*;
		                                                                           """, objectValues);

		return await results.ToArrayAsync();
	}

	/// <summary>
	///		Upserts <see cref="ProjectMenuItem"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="projectMenuItems"></param>
	/// <returns></returns>
	public static async Task<ProjectMenuItem[]> UpsertProjectMenuItems(this VoltProjectDbContext dbContext,
		ProjectMenuItem[] projectMenuItems)
	{
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectMenuItems, x => new { x.ProjectVersionId, x.Title, x.ItemOrder, x.Href }, false);

		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		
		return await dbContext.ProjectMenuItems.FromSqlRaw($"""
		                                       MERGE INTO public.project_menu_item AS pmi
		                                       	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_version_id, title, item_order, href)) AS menu_items_values
		                                       	ON menu_items_values.project_version_id = pmi.project_version_id
		                                       	AND menu_items_values.href = pmi.href
		                                       WHEN NOT MATCHED THEN
		                                       	INSERT (project_version_id, title, item_order, href)
		                                       	VALUES (menu_items_values.project_version_id, menu_items_values.title, menu_items_values.item_order, menu_items_values.href)
		                                       WHEN MATCHED THEN
		                                       	UPDATE SET 
		                                       		title = menu_items_values.title,
		                                       		item_order = menu_items_values.item_order
		                                       RETURNING pmi.*;	
		                                       """, objectValues).ToArrayAsync();
	}

	/// <summary>
	///		Bulk insert <see cref="ProjectBuildEventLog"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="projectBuildEventLogs"></param>
	public static async Task InsertProjectBuildEventLogs(this VoltProjectDbContext dbContext,
		ProjectBuildEventLog[] projectBuildEventLogs)
	{
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectBuildEventLogs, x => new { x.BuildEventId, x.Message, x.Date, x.LogLevelId }, false);

		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		await dbContext.Database.ExecuteSqlRawAsync($"""
		                                       INSERT INTO public.project_build_event_log
		                                       (build_event_id, message, "date", log_level_id)
		                                       VALUES{paramsPlaceholder};
		                                       """, objectValues);
	}

	/// <summary>
	///		Upserts <see cref="ProjectPage"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="projectPages"></param>
	/// <returns></returns>
	public static async Task<ProjectPage[]> UpsertProjectPages(this VoltProjectDbContext dbContext,
		ProjectPage[] projectPages)
	{
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectPages, x => new { x.ProjectVersionId, x.Path, x.Published, x.PublishedDate, x.Title, x.TitleDisplay, x.WordCount, x.ProjectTocId, x.TocRel, x.GitUrl, x.Aside, x.Metabar, x.Description, x.Content, x.PageHash, x.LanguageConfiguration }, false);
		
		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		return await dbContext.ProjectPages.FromSqlRaw($"""
		                                         MERGE INTO public.project_page AS pp
		                                            	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_version_id, path, published, published_date, title, title_display, word_count, project_toc_id, toc_rel, git_url, aside, metabar, description, content, page_hash, language_configuration)) AS page_items_value
		                                            	ON page_items_value.project_version_id = pp.project_version_id
		                                            	AND page_items_value.path = pp.path
		                                         WHEN NOT MATCHED THEN
		                                         	INSERT (project_version_id, path, published, published_date, title, title_display, word_count, project_toc_id, toc_rel, git_url, aside, metabar, description, content, page_hash, language_configuration)
		                                            	VALUES (page_items_value.project_version_id, page_items_value.path, page_items_value.published, page_items_value.published_date, page_items_value.title, page_items_value.title_display, page_items_value.word_count::int, page_items_value.project_toc_id::int, page_items_value.toc_rel, page_items_value.git_url, page_items_value.aside, page_items_value.metabar, page_items_value.description, page_items_value.content, page_items_value.page_hash, page_items_value.language_configuration)
		                                         WHEN MATCHED THEN
		                                            	UPDATE SET
		                                            		published = page_items_value.published,
		                                            		title = page_items_value.title,
		                                            		title_display = page_items_value.title_display,
		                                            		word_count = page_items_value.word_count::int,
		                                            		project_toc_id = page_items_value.project_toc_id::int,
		                                            		toc_rel = page_items_value.toc_rel,
		                                            		git_url = page_items_value.git_url,
		                                            		aside = page_items_value.aside,
		                                            		metabar = page_items_value.metabar,
		                                            		description = page_items_value.description,
		                                            		page_hash = page_items_value.page_hash,
		                                            		language_configuration = page_items_value.language_configuration
		                                         RETURNING pp.*;
		                                         """, objectValues).ToArrayAsync();
	}
	
	/// <summary>
	///     Upsert <see cref="ProjectPageStorageItem"/>s
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="pageStorageItems"></param>
	public static async Task UpsertProjectPageStorageItems(this VoltProjectDbContext dbContext, ProjectPageStorageItem[] pageStorageItems)
	{
		(object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(pageStorageItems, x => new { x.PageId, x.StorageItemId }, false);
		string paramsPlaceholder = string.Join(",", objectPlaceholders);
        
#pragma warning disable EF1002
		await dbContext.Database.ExecuteSqlRawAsync($"""
		                                             MERGE INTO public.project_page_storage_item AS ppsi
		                                             	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(page_id, storage_item_id)) AS storage_item_values
		                                             	ON storage_item_values.page_id = ppsi.page_id
		                                             	AND storage_item_values.storage_item_id = ppsi.storage_item_id
		                                             WHEN NOT MATCHED THEN 
		                                             	INSERT (page_id, storage_item_id)
		                                             	VALUES (storage_item_values.page_id, storage_item_values.storage_item_id);
		                                             """, objectValues!);
#pragma warning restore EF1002
	}
	
	/// <summary>
    ///     Upsert <see cref="ProjectStorageItem"/>s
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="storageItems"></param>
    /// <returns></returns>
    public static async Task<ProjectStorageItem[]> UpsertProjectStorageAssets(this VoltProjectDbContext dbContext, ProjectStorageItem[] storageItems)
    {
        (object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(storageItems, x => new { x.ProjectVersionId, x.Path, x.Hash }, false);
        string paramsPlaceholder = string.Join(",", objectPlaceholders);

#pragma warning disable EF1002
        return await dbContext.ProjectStorageItems.FromSqlRaw($"""
                                                               MERGE INTO public.project_storage_item AS psi
                                                               	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_version_id, path, hash)) AS storage_item_values
                                                               	ON storage_item_values.project_version_id = psi.project_version_id
                                                               	AND storage_item_values.PATH = psi.PATH
                                                               WHEN NOT MATCHED THEN 
                                                               	INSERT (project_version_id, path, hash)
                                                               	VALUES (storage_item_values.project_version_id, storage_item_values.path, storage_item_values.hash)
                                                               WHEN MATCHED THEN
                                                               	UPDATE SET
                                                               		hash = storage_item_values.hash
                                                               RETURNING psi.*;
                                                               """, objectValues!)
            .AsNoTracking()
            .ToArrayAsync();
#pragma warning restore EF1002
    }
	
	/// <summary>
	///     Upsert <see cref="ProjectExternalItemStorageItem"/>s
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="pageStorageItems"></param>
	public static async Task UpsertProjectExternalItemStorageItemItems(this VoltProjectDbContext dbContext, ProjectExternalItemStorageItem[] pageStorageItems)
	{
		(object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(pageStorageItems, x => new { x.ProjectExternalItemId, x.StorageItemId }, false);
		string paramsPlaceholder = string.Join(",", objectPlaceholders);
        
#pragma warning disable EF1002
		await dbContext.Database.ExecuteSqlRawAsync($"""
		                                             MERGE INTO public.project_external_item_storage_item AS eisi
		                                             	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_external_item_id, storage_item_id)) AS storage_item_values
		                                             	ON storage_item_values.project_external_item_id = eisi.project_external_item_id
		                                             	AND storage_item_values.storage_item_id = eisi.storage_item_id
		                                             WHEN NOT MATCHED THEN 
		                                             	INSERT (project_external_item_id, storage_item_id)
		                                             	VALUES (storage_item_values.project_external_item_id, storage_item_values.storage_item_id);

		                                             """, objectValues!);
#pragma warning restore EF1002
	}

	/// <summary>
	///		Upsert <see cref="ProjectPageBreadcrumb"/>
	/// </summary>
	/// <param name="dbContext"></param>
	/// <param name="breadcrumbs"></param>
	public static async Task<ProjectPageBreadcrumb[]> UpsertProjectPageBreadcrumbs(this VoltProjectDbContext dbContext,
		ProjectPageBreadcrumb[] breadcrumbs)
	{
		(object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(breadcrumbs, x => new { x.ProjectPageId, x.Title, x.Href, x.BreadcrumbOrder }, false);
		string paramsPlaceholder = string.Join(",", objectPlaceholders);

		return await dbContext.ProjectPageBreadcrumbs.FromSqlRaw($"""
		                                                   MERGE INTO public.project_page_breadcrumb AS tgt
		                                                   	USING (SELECT * FROM (VALUES{paramsPlaceholder}) AS s(project_page_id, title, href, breadcrumb_order)) AS src
		                                                   	ON tgt.href = src.href
		                                                   	AND tgt.project_page_id = src.project_page_id
		                                                   	AND tgt.title = src.title
		                                                   WHEN MATCHED THEN UPDATE SET
		                                                   	title = src.title,
		                                                   	breadcrumb_order = src.breadcrumb_order
		                                                   WHEN NOT MATCHED THEN
		                                                   	INSERT (project_page_id, title, href, breadcrumb_order)
		                                                   	VALUES (src.project_page_id, src.title, src.href, src.breadcrumb_order)
		                                                   RETURNING tgt.*;
		                                                   """, objectValues).ToArrayAsync();
	}
}