using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

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

	public static async Task DeleteProjectTocItemsNotInValues(this VoltProjectDbContext dbContext,
		ProjectTocItem[] projectTocItems, int projectVersionId)
	{
		//Allocate first value to be project version id
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectTocItems, x => new { x.Id }, false, 1);
		objectValues[0] = projectVersionId;
		
		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		
		await dbContext.Database.ExecuteSqlRawAsync($"""
		                                       DELETE FROM public.project_toc_item
		                                       WHERE id NOT IN (VALUES{paramsPlaceholder})
		                                       AND project_version_id = @p0;
		                                       """, objectValues);
	}

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
	
	public static async Task DeleteProjectMenuItemsNotInValues(this VoltProjectDbContext dbContext, ProjectMenuItem[] projectMenuItems, int projectVersionId)
	{
		(object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(projectMenuItems, x => new { x.Id }, false, 1);
		objectValues[0] = projectVersionId;
		
		string paramsPlaceholder = string.Join(",", objectPlaceholders);
		
		await dbContext.Database.ExecuteSqlRawAsync($"""
		                                             DELETE FROM public.project_menu_item
		                                             WHERE id NOT IN (VALUES{paramsPlaceholder})
		                                             AND project_version_id = @p0;
		                                             """, objectValues);
	}
}