using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Infrastructure.Persistence.Context;

namespace EkubCircle.Tests.Common;

public static class TestDbContextFactory
{
    public static (EkubDbContext Context, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<EkubDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new EkubDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
