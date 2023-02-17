using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoltProjects.Server.Models;

namespace VoltProjects.Server.Services;

public class VoltProjectsDbContext : DbContext
{
    public VoltProjectsDbContext(DbContextOptions<VoltProjectsDbContext> options) : base(options)
    {
    }
    
    public DbSet<DocBuilder> DocBuilders { get; set; }
    
    public DbSet<DocView> DocViews { get; set; }

    public DbSet<Project> Projects { get; set; }
    
    public DbSet<ProjectBuildEvent> ProjectBuildEvents { get; set; }
    
    public DbSet<ProjectVersion> ProjectVersions { get; set; }
    
    public DbSet<ProjectLanguage> ProjectLanguages { get; set; }
    
    public DbSet<Language> Languages { get; set; }

    public DbSet<PreBuildCommand> PreBuildCommands { get; set; }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        VoltProjectsDbContext context = services.GetRequiredService<VoltProjectsDbContext>();
        
        context.Database.EnsureCreated();
        
        if(context.Languages.Any())
            return;

        //Languages
        Language language = new()
        {
            Name = "en"
        };
        
        context.Languages.Add(language);
        
        //Doc builders
        DocBuilder docBuilder = new()
        {
            Id = "vdocfx",
            Name = "VDocFx"
        };
        context.DocBuilders.Add(docBuilder);
        
        //Doc Views
        DocView docView = new()
        {
            Id = "vdocfx",
            Name = "VDocFx"
        };
        context.DocViews.Add(docView);
        
        context.SaveChanges();
    }
}