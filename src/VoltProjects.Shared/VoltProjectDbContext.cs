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
    
    public DbSet<ProjectBuildSchedule> ProjectBuildSchedules { get; set; }
    
    public DbSet<ProjectVersion> ProjectVersions { get; set; }
    
    public DbSet<ProjectPage> ProjectPages { get; set; }
    
    public DbSet<ProjectPageContributor> ProjectPageContributors { get; set; }
    
    public DbSet<ProjectPageStorageItem> ProjectPageStorageItems { get; set; }

    public DbSet<ProjectMenuItem> ProjectMenuItems { get; set; }
    
    public DbSet<ProjectToc> ProjectTocs { get; set; }
    
    public DbSet<ProjectTocItem> ProjectTocItems { get; set; }
    
    public DbSet<ProjectStorageItem> ProjectStorageItems { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<ProjectPreBuild> PreBuildCommands { get; set; }
    
    public DbSet<ProjectExternalItem> ProjectExternalItems { get; set; }
    
    public DbSet<ProjectExternalItemStorageItem> ProjectExternalItemStorageItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Default Values
        {
            //Language
            modelBuilder.Entity<Language>()
                .Property(p => p.Configuration)
                .HasDefaultValueSql("'english'");
            
            //Project
            modelBuilder.Entity<Project>()
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Project>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<ProjectBuildSchedule>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");
            
            modelBuilder.Entity<ProjectBuildSchedule>()
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");
            
            //ProjectVersion
            modelBuilder.Entity<ProjectVersion>()
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<ProjectVersion>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");
            
            //ProjectPage
            modelBuilder.Entity<ProjectPage>()
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<ProjectPage>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<ProjectPage>()
                .Property(p => p.LanguageConfiguration)
                .HasDefaultValueSql("'english'");
            
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
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");
            
            modelBuilder.Entity<ProjectBuildEvent>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");
            
            //External Item
            modelBuilder.Entity<ProjectExternalItem>()
                .Property(p => p.CreationTime)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<ProjectExternalItem>()
                .Property(p => p.LastUpdateTime)
                .HasDefaultValueSql("now()");
        }
        
        //Unique Keys
        {
            //Project Unique Keys
            modelBuilder.Entity<Project>()
                .Property(p => p.Id).UseIdentityAlwaysColumn();
            
            modelBuilder.Entity<Project>()
                .HasIndex(p => new { p.Name })
                .IsUnique();
            
            //Project Build Event
            modelBuilder.Entity<ProjectBuildEvent>()
                .Property(p => p.Id)
                .UseIdentityAlwaysColumn();
            
            //ProjectBuildSchedule
            modelBuilder.Entity<ProjectBuildSchedule>()
                .Property(p => p.Id)
                .UseIdentityAlwaysColumn();
            
            //Project Menu
            modelBuilder.Entity<ProjectMenuItem>()
                .Property(p => p.Id).UseIdentityAlwaysColumn();
            
            modelBuilder.Entity<ProjectMenuItem>()
                .HasIndex(p => new { p.ProjectVersionId, p.Href })
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
            
            //Project Page Storage Item
            modelBuilder.Entity<ProjectPageStorageItem>()
                .HasIndex(p => new { p.PageId, p.StorageItemId })
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
            
            //ProjectTocItem
            modelBuilder.Entity<ProjectTocItem>()
                .HasIndex(p => new { p.ProjectTocId, p.ProjectVersionId, p.Title, p.ParentTocItemId, p.Href })
                .IsUnique()
                .AreNullsDistinct(false);
            
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
            
            //External Items
            modelBuilder.Entity<ProjectExternalItem>()
                .HasIndex(p => new { p.ProjectVersionId, p.Path })
                .IsUnique();
            
            //External Item Storage Items
            modelBuilder.Entity<ProjectExternalItemStorageItem>()
                .HasIndex(p => new { p.ProjectExternalItemId, p.StorageItemId })
                .IsUnique();
        }
        
        
        //Checks
        {
            modelBuilder.Entity<ProjectPage>()
                .ToTable(t => t.HasCheckConstraint("ck_toc_nullability_same",
                    "(project_toc_id IS NULL AND toc_rel IS NULL) OR (project_toc_id IS NOT NULL AND toc_rel IS NOT NULL)"));
        }
        
        
        //FKs
        {
            //Project
            //modelBuilder.Entity<Project>().HasMany(e => e.ProjectVersions).WithOne(e => e.Project)
            //    .HasForeignKey(x => x.Project).OnDelete(DeleteBehavior.Restrict);
            
            //ProjectBuildEvent
            modelBuilder.Entity<ProjectBuildEvent>()
                .HasOne(p => p.Project)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectBuildSchedule>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectExternalItem
            modelBuilder.Entity<ProjectExternalItem>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectExternalItemStorageItem
            modelBuilder.Entity<ProjectExternalItemStorageItem>()
                .HasOne(p => p.ProjectExternalItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectExternalItemStorageItem>()
                .HasOne(p => p.StorageItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            //ProjectMenuItem
            modelBuilder.Entity<ProjectMenuItem>()
                .HasOne(p => p.ProjectVersion)
                .WithMany(p => p.MenuItems)
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectPage
            modelBuilder.Entity<ProjectPage>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectPage>()
                .HasOne(p => p.ProjectToc)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectPageContributor
            modelBuilder.Entity<ProjectPageContributor>()
                .HasOne(p => p.Page)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectPageStorageItem
            modelBuilder.Entity<ProjectPageStorageItem>()
                .HasOne(p => p.Page)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ProjectPageStorageItem>()
                .HasOne(p => p.StorageItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectPreBuild
            modelBuilder.Entity<ProjectPreBuild>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectStorageItem
            modelBuilder.Entity<ProjectStorageItem>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectToc
            modelBuilder.Entity<ProjectToc>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            
            //ProjectTocItem
            modelBuilder.Entity<ProjectTocItem>()
                .HasOne(p => p.ProjectToc)
                .WithMany(x => x.TocItems)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTocItem>()
                .HasOne(p => p.ProjectVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTocItem>()
                .HasOne(p => p.ParentTocItem)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
            
            //ProjectVersion
            modelBuilder.Entity<ProjectVersion>()
                .HasOne(p => p.Project)
                .WithMany(p => p.ProjectVersions)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ProjectVersion>()
                .HasOne(p => p.DocBuilder)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectVersion>()
                .HasOne(p => p.Language)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            
        }
        
        //Collations
        {
            modelBuilder.HasCollation("vp_collation_nondeterministic", "en-u-ks-primary", "icu", false);

            modelBuilder.Entity<Project>()
                .Property(p => p.Name)
                .UseCollation("vp_collation_nondeterministic");

            modelBuilder.Entity<ProjectVersion>()
                .Property(p => p.VersionTag)
                .UseCollation("vp_collation_nondeterministic");
        }
        
        //Seed data
        {
            //Doc Builder
            modelBuilder.Entity<DocBuilder>()
                .HasData(new DocBuilder
                {
                    Id = "docfx",
                    Name = "DocFx",
                    Application = "docfx",
                    Arguments = ["build", "--exportRawModel"]
                }, new DocBuilder
                {
                    Id = "mkdocs",
                    Name = "MkDocs",
                    Application = "python",
                    Arguments = ["-m mkdocs", "build"]
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
}