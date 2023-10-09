using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Medallion.Threading.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Shared;

/// <summary>
///     VoltProject <see cref="DbContext"/> extensions
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///     Adds <see cref="VoltProjectDbContext"/> to a <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="typePrefix"></param>
    /// <returns></returns>
    public static IServiceCollection UseVoltProjectDbContext(this IServiceCollection services,
        IConfiguration configuration, string typePrefix)
    {
        string connectionStringName = $"{typePrefix}Connection";
        string? connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new NullReferenceException($"Connection string for {typePrefix} was not provided!");
        
        return services
            .AddDbContextFactory<VoltProjectDbContext>(
                options =>
                    options.UseNpgsql(connectionString))
            .AddDbContext<VoltProjectDbContext>(
                options =>
                    options.UseNpgsql(connectionString));
    }

    public static IHost HandleDbMigrations(this IHost host)
    {
        IServiceProvider services = host.Services.CreateScope().ServiceProvider;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbContextExtensions));
        VoltProjectDbContext dbContext = services.GetRequiredService<VoltProjectDbContext>();

        logger.LogInformation("Checking database...");

        string? connectionString = dbContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new NullReferenceException("Connection string for DB is null!");
        
        //PostgresDistributedLock uses Postgres's Advisory Locks.
        //Upto the app to respect the lock
        //
        //https://www.postgresql.org/docs/9.4/explicit-locking.html#ADVISORY-LOCKS
        PostgresDistributedLock migrationLock =
            new(new PostgresAdvisoryLockKey("MigrationsLock", true), connectionString);
        using (migrationLock.Acquire())
        {
            bool pendingMigrations = dbContext.Database.GetPendingMigrations().Any();

            if (pendingMigrations)
            {
                logger.LogWarning("Database requires migrations! Migrating...");
                IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
                transaction.CreateSavepoint("Migrations");
                try
                {
                    dbContext.Database.Migrate();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("Migrations");
                    logger.LogError("An error occured while migrating the db!", ex);
                    throw;
                }
            }
        }
        
        return host;
    }

    /// <summary>
    ///     Generates params for a raw SQL function to use
    /// </summary>
    /// <param name="values"></param>
    /// <param name="paramsExpression"></param>
    /// <param name="includeRow"></param>
    /// <param name="paramCountAllocStartIndex"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    internal static (object?[], string[]) GenerateParams<TEntity>(TEntity[] values, Expression<Func<TEntity, object?>> paramsExpression, bool includeRow = true, int paramCountAllocStartIndex = 0)
    {
        IReadOnlyList<PropertyInfo> properties = paramsExpression.GetPropertyAccessList();
        int propertiesCount = properties.Count;
        int valuesCount = values.Length;
        object?[] paramValues = new object[paramCountAllocStartIndex + valuesCount * propertiesCount];

        int startIndex = paramCountAllocStartIndex;
        string[] rows = new string[valuesCount];
        for (int i = 0; i < valuesCount; i++)
        {
            TEntity value = values[i];
            
            rows[i] = $"{(includeRow ? "ROW" : "")}({string.Join(",", Enumerable.Range(startIndex, propertiesCount).Select(x => $"@p{x}"))})";
            
            for (int j = 0; j < propertiesCount; j++)
            {
                PropertyInfo propertyInfo = properties[j];
                
                //If item is meant to be json, parse it before hand
                ColumnAttribute? columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute is { TypeName: "jsonb" })
                    paramValues[startIndex] = JsonSerializer.Serialize(propertyInfo.GetValue(value));
                else
                    paramValues[startIndex] = propertyInfo.GetValue(value);

                startIndex++;
            }
        }

        return (paramValues, rows);
    }
}