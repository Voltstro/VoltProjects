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

    public DbSet<ProjectMenu> ProjectMenus { get; set; }
    
    public DbSet<ProjectToc> ProjectTocs { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<ProjectPreBuild> PreBuildCommands { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}