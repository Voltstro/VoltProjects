using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared.Models;

namespace VoltProjects.Shared;

public class VoltProjectDbContext : DbContext
{
    public VoltProjectDbContext()
    {
    }
    
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
        optionsBuilder.UseNpgsql();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Project Unique Keys
        modelBuilder.Entity<Project>()
            .HasIndex(p => new { p.Name })
            .IsUnique();
        
        //Project Version Unique Keys
        modelBuilder.Entity<ProjectVersion>()
            .HasIndex(p => new { p.ProjectId, p.VersionTag, p.LanguageId })
            .IsUnique();
        
        modelBuilder.Entity<ProjectVersion>()
            .HasIndex(p => new { p.ProjectId, p.VersionTag, p.LanguageId, p.IsDefault })
            .HasFilter("\"IsDefault\" = true")
            .IsUnique();
        
        //Project Page Unique Keys
        modelBuilder.Entity<ProjectPage>()
            .HasIndex(p => new { p.ProjectVersionId, p.Path })
            .IsUnique();
        
        //Project Page Contributor
        modelBuilder.Entity<ProjectPageContributor>()
            .HasIndex(p => new { p.PageId })
            .IsUnique();
        
        modelBuilder.Entity<ProjectPageContributor>()
            .HasIndex(p => new { p.PageId, p.GitHubUserId })
            .IsUnique();
        
        //Project Menu Unique Keys
        modelBuilder.Entity<ProjectMenu>()
            .HasIndex(p => new { p.ProjectVersionId })
            .IsUnique();
        
        //project TOC Unique Keys
        modelBuilder.Entity<ProjectToc>()
            .HasIndex(p => new { p.ProjectVersionId, p.TocRel })
            .IsUnique();
        
        //Seed Data
        modelBuilder.Entity<Language>().HasData(new Language
        {
            Id = 1,
            Name = "en"
        });

        modelBuilder.Entity<DocBuilder>().HasData(new DocBuilder
        {
            Id = "vdocfx",
            Name = "VDocFx",
            Application = "vdocfx",
            Arguments = new []{"build", "--output-type PageJson", "--output {0}"},
            EnvironmentVariables = new []{"DOCS_GITHUB_TOKEN="}
        });
    }
}