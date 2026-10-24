using Microsoft.EntityFrameworkCore;
using VoltProjects.Shared;

namespace VoltProjects.Tests;

public static class TestDatabase
{
    private static string? dbConnectionString;
    
    public static void Init(string connectionString)
    {
        dbConnectionString = connectionString;
    }
    
    public static VoltProjectDbContext CreateContext()
    {
        if (dbConnectionString == null)
            throw new InvalidOperationException("Test database not initialized!");
        
        DbContextOptions<VoltProjectDbContext> options = new DbContextOptionsBuilder<VoltProjectDbContext>()
            .UseNpgsql(dbConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        
        return new VoltProjectDbContext(options);
    }
}