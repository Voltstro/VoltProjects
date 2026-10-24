using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using VoltProjects.Shared;

namespace VoltProjects.Tests.Core;

/// <summary>
///     Base class for any tests needing a <see cref="VoltProjectDbContext"/>.
///     Will create a <see cref="VoltProjectDbContext"/> for each test and start a transaction
/// </summary>
public abstract class DbContextTest
{
    protected VoltProjectDbContext DbContext;
    
    private IDbContextTransaction transaction;
    
    [SetUp]
    public virtual void Init()
    {
        DbContext = TestDatabase.CreateContext();
        transaction = DbContext.Database.BeginTransaction();
    }

    [TearDown]
    public virtual void Cleanup()
    {
        transaction.Dispose();
        DbContext.Dispose();
    }
}