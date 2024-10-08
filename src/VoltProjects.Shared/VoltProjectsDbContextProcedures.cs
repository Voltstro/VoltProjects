using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared.Models;

namespace VoltProjects.Shared;

/// <summary>
///     Extension class for calling procedures on <see cref="VoltProjectDbContext"/>
/// </summary>
public static class VoltProjectsDbContextProcedures
{
    /// <summary>
    ///     Upsert <see cref="ProjectToc"/>s against a <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="tocs"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    public static async Task<ProjectToc[]> UpsertProjectTOCs(this VoltProjectDbContext dbContext, ProjectToc[] tocs,
        ProjectVersion projectVersion)
    {
        (object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(tocs, x => new { x.TocRel, x.TocItem }, true, 1);
        objectValues[0] = projectVersion.Id;
        
        string paramsPlaceholder = string.Join(",", objectPlaceholders);

#pragma warning disable EF1002
        return await dbContext.ProjectTocs
            .FromSqlRaw(
                $"SELECT * FROM public.upsert_project_tocs(@p0, ARRAY[{paramsPlaceholder}]::upsertedtoc[]);", objectValues!)
            .AsNoTracking()
            .ToArrayAsync();
#pragma warning restore EF1002
    }

    /// <summary>
    ///     Upsert a <see cref="ProjectMenu"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="projectMenu"></param>
    public static async Task UpsertProjectMenu(this VoltProjectDbContext dbContext, ProjectMenu projectMenu)
    {
        string linkItemJson = JsonSerializer.Serialize(projectMenu.LinkItem);
        await dbContext.Database.ExecuteSqlRawAsync("""
                                                     INSERT INTO public.project_menu
                                                        (project_version_id, link_item)
                                                     VALUES (@p0, @p1::jsonb)
                                                     ON CONFLICT (project_version_id)
                                                     DO UPDATE SET
                                                        link_item = EXCLUDED.link_item;
                                                     """, projectMenu.ProjectVersionId, linkItemJson);
    }

    /// <summary>
    ///     Upsert <see cref="ProjectPage"/>s against a <see cref="ProjectVersion"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="pages"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    public static async Task<ProjectPage[]> UpsertProjectPages(this VoltProjectDbContext dbContext, ProjectPage[] pages, ProjectVersion projectVersion)
    {
        (object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(pages, x => new { x.PublishedDate, x.Title, x.TitleDisplay, x.GitUrl, x.Aside, x.Metabar, x.WordCount, x.ProjectTocId, x.TocRel, x.Path, x.Description, x.Content, x.PageHash, x.LanguageConfiguration }, true, 1);
        objectValues[0] = projectVersion.Id;
        
        string paramsPlaceholder = string.Join(",", objectPlaceholders);
        
#pragma warning disable EF1002
        return await dbContext.ProjectPages
            .FromSqlRaw($"SELECT * FROM public.upsert_project_pages(@p0, ARRAY[{paramsPlaceholder}]::upsertedpage[]);", objectValues!)
            .AsNoTracking()
            .ToArrayAsync();
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
                                                               INSERT INTO public.project_storage_item 
                                                               (project_version_id, path, hash)
                                                               VALUES {paramsPlaceholder}
                                                               ON CONFLICT (project_version_id, path)
                                                               DO UPDATE SET 
                                                                hash = EXCLUDED.hash
                                                               RETURNING *;
                                                               """, objectValues!)
            .AsNoTracking()
            .ToArrayAsync();
#pragma warning restore EF1002
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
                                                     INSERT INTO public.project_page_storage_item
                                                     (page_id, storage_item_id)
                                                     VALUES {paramsPlaceholder}
                                                     ON CONFLICT DO NOTHING;
                                                     """, objectValues!);
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
                                                     INSERT INTO public.project_external_item_storage_item
                                                     (project_external_item_id, storage_item_id)
                                                     VALUES {paramsPlaceholder}
                                                     ON CONFLICT DO NOTHING;
                                                     """, objectValues!);
#pragma warning restore EF1002
    }
}