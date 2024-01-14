using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared.Models;

namespace VoltProjects.Shared;

/// <summary>
///     Main VoltProject's <see cref="DbContext"/>
/// </summary>
public sealed class VoltProjectDbContext : DbContext
{
    /// <summary>
    ///     Creates a new <see cref="VoltProjectDbContext"/> instance
    /// </summary>
    public VoltProjectDbContext()
    {
    }
    
    /// <summary>
    ///     Creates a new <see cref="VoltProjectDbContext"/> instance
    /// </summary>
    public VoltProjectDbContext(DbContextOptions<VoltProjectDbContext> options) : base(options)
    {
    }
    
    public DbSet<DocBuilder> DocBuilders { get; set; }

    public DbSet<Project> Projects { get; set; }
    
    public DbSet<ProjectBuildEvent> ProjectBuildEvents { get; set; }
    
    public DbSet<ProjectVersion> ProjectVersions { get; set; }
    
    public DbSet<ProjectPage> ProjectPages { get; set; }
    
    public DbSet<ProjectPageContributor> ProjectPageContributors { get; set; }
    
    public DbSet<ProjectPageStorageItem> ProjectPageStorageItems { get; set; }

    public DbSet<ProjectMenu> ProjectMenus { get; set; }
    
    public DbSet<ProjectToc> ProjectTocs { get; set; }
    
    public DbSet<ProjectStorageItem> ProjectStorageItems { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<ProjectPreBuild> PreBuildCommands { get; set; }
    
    /// <summary>
    ///     Upserts <see cref="ProjectToc"/>s
    /// </summary>
    /// <param name="tocs"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    public async Task<ProjectToc[]> UpsertProjectTOCs(ProjectToc[] tocs, ProjectVersion projectVersion)
    {
        (object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(tocs, x => new { x.TocRel, x.TocItem }, true, 1);
        objectValues[0] = projectVersion.Id;
        
        string paramsPlaceholder = string.Join(",", objectPlaceholders);

        return await ProjectTocs.FromSqlRaw(
                $"SELECT * FROM public.upsert_project_tocs(@p0, ARRAY[{paramsPlaceholder}]::upsertedtoc[]);", objectValues)
            .AsNoTracking()
            .ToArrayAsync();
    }

    /// <summary>
    ///     Upserts <see cref="ProjectPage"/>
    /// </summary>
    /// <param name="pages"></param>
    /// <param name="projectVersion"></param>
    public async Task<ProjectPage[]> UpsertProjectPages(ProjectPage[] pages, ProjectVersion projectVersion)
    {
        (object?[] objectValues, string[] objectPlaceholders) = DbContextExtensions.GenerateParams(pages, x => new { x.PublishedDate, x.Title, x.TitleDisplay, x.GitUrl, x.Aside, x.Metabar, x.WordCount, x.ProjectTocId, x.TocRel, x.Path, x.Description, x.Content, x.PageHash }, true, 1);
        objectValues[0] = projectVersion.Id;
        
        string paramsPlaceholder = string.Join(",", objectPlaceholders);

        return await ProjectPages
            .FromSqlRaw($"SELECT * FROM public.upsert_project_pages(@p0, ARRAY[{paramsPlaceholder}]::upsertedpage[]);",
                objectValues).AsNoTracking().ToArrayAsync();
    }

    public async Task<ProjectStorageItem[]> UpsertProjectStorageAssets(ProjectStorageItem[] storageItems)
    {
        (object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(storageItems, x => new { x.ProjectVersionId, x.Path, x.Hash }, false);
        string paramsPlaceholder = string.Join(",", objectPlaceholders);

        return await ProjectStorageItems.FromSqlRaw(@$"
INSERT INTO public.project_storage_item 
(project_version_id, path, hash)
VALUES {paramsPlaceholder}
ON CONFLICT (project_version_id, path)
DO UPDATE SET 
	hash = EXCLUDED.hash
RETURNING *;
", objectValues).AsNoTracking().ToArrayAsync();
        
        //await Database.ExecuteSqlRawAsync(, objectValues);
    }

    public async Task UpsertProjectPageStorageItems(ProjectPageStorageItem[] pageStorageItems)
    {
        (object?[] objectValues, string[] objectPlaceholders)  = DbContextExtensions.GenerateParams(pageStorageItems, x => new { x.PageId, x.StorageItemId }, false);
        string paramsPlaceholder = string.Join(",", objectPlaceholders);
        
        await Database.ExecuteSqlRawAsync($@"
INSERT INTO public.project_page_storage_item
(page_id, storage_item_id)
VALUES {paramsPlaceholder}
ON CONFLICT DO NOTHING;
", objectValues);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Default Values
        
        //Project
        modelBuilder.Entity<Project>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Project>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Version
        modelBuilder.Entity<ProjectVersion>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectVersion>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Menu
        modelBuilder.Entity<ProjectMenu>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectMenu>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Page
        modelBuilder.Entity<ProjectPage>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectPage>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project TOC
        modelBuilder.Entity<ProjectToc>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectToc>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Storage Item
        modelBuilder.Entity<ProjectStorageItem>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectStorageItem>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Pre Build
        modelBuilder.Entity<ProjectPreBuild>()
            .Property(p => p.LastUpdateTime)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<ProjectPreBuild>()
            .Property(p => p.CreationTime)
            .HasDefaultValueSql("now()");
        
        //Project Build Event
        modelBuilder.Entity<ProjectBuildEvent>()
            .Property(p => p.Date)
            .HasDefaultValueSql("now()");
        
        
        //Unique Keys
        
        //Project Unique Keys
        modelBuilder.Entity<Project>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<Project>()
            .HasIndex(p => new { p.Name })
            .IsUnique();
        
        //Project Build Event
        modelBuilder.Entity<ProjectBuildEvent>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        //Project Menu
        modelBuilder.Entity<ProjectMenu>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<ProjectMenu>()
            .HasIndex(p => new { p.ProjectVersionId })
            .IsUnique();

        //Project Page Unique Keys
        modelBuilder.Entity<ProjectPage>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<ProjectPage>()
            .HasIndex(p => new { p.ProjectVersionId, p.Path })
            .IsUnique();
        
        //Project Page Contributor
        modelBuilder.Entity<ProjectPageContributor>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<ProjectPageContributor>()
            .HasIndex(p => new { p.PageId })
            .IsUnique();
        
        modelBuilder.Entity<ProjectPageContributor>()
            .HasIndex(p => new { p.PageId, p.GitHubUserId })
            .IsUnique();
        
        //Project Pre Build
        modelBuilder.Entity<ProjectPreBuild>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        //Storage Item
        modelBuilder.Entity<ProjectStorageItem>()
            .HasIndex(p => new { p.ProjectVersionId, p.Path })
            .IsUnique();

        //Project TOC
        modelBuilder.Entity<ProjectToc>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<ProjectToc>()
            .HasIndex(p => new { p.ProjectVersionId, p.TocRel })
            .IsUnique();
        
        //Project Version
        modelBuilder.Entity<ProjectVersion>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<ProjectVersion>()
            .HasIndex(p => new { p.ProjectId, p.VersionTag, p.LanguageId })
            .IsUnique();
        
        modelBuilder.Entity<ProjectVersion>()
            .HasIndex(p => new { p.ProjectId, p.VersionTag, p.LanguageId, p.IsDefault })
            .HasFilter("is_default = true")
            .IsUnique();
        
        //Seed data
        
        //Doc Builder
        modelBuilder.Entity<DocBuilder>()
            .HasData(new DocBuilder
            {
                Id = "vdocfx",
                Name = "VDocFx",
                Application = "vdocfx",
                Arguments = new []{"build", "--output-type PageJson", "--output {0}"},
                EnvironmentVariables = new []{"DOCS_GITHUB_TOKEN="}
            });
        
        //Language
        modelBuilder.Entity<Language>()
            .Property(p => p.Id).UseIdentityAlwaysColumn();
        
        modelBuilder.Entity<Language>()
            .HasData(new Language
            {
                Id = 1,
                Name = "en"
            });
    }
}