using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    /// <returns></returns>
    public static IServiceCollection UseVoltProjectDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddDbContextFactory<VoltProjectDbContext>(
                options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
            .AddDbContext<VoltProjectDbContext>(
                options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }

    /// <summary>
    ///     Generates params for a raw SQL function to use
    /// </summary>
    /// <param name="values"></param>
    /// <param name="paramsExpression"></param>
    /// <param name="paramCountAllocStartIndex"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    internal static (object?[], string[]) GenerateParams<TEntity>(TEntity[] values, Expression<Func<TEntity, object?>> paramsExpression, int paramCountAllocStartIndex = 0)
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
            
            rows[i] = $"ROW({string.Join(",", Enumerable.Range(startIndex, propertiesCount).Select(x => $"@p{x}"))})";
            
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