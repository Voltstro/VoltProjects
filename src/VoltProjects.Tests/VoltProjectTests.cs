using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using VoltProjects.Shared;

namespace VoltProjects.Tests;

[SetUpFixture]
public class VoltProjectTests
{
    private PostgreSqlContainer postgres;
    private ILogger logger;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        logger = new NullLogger<VoltProjectTests>();
        
        //Create the postgres container
        postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .Build();

        await postgres.StartAsync();
        TestDatabase.Init(postgres.GetConnectionString());
        
        //Migrate DB
        VoltProjectDbContext context = TestDatabase.CreateContext();
        
        DbContextExtensions.DoMigrations(logger, context);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await postgres.DisposeAsync();
    }
}