using ExpenseWise.DependencyConfiguration;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Tests.Infrastructure.SqlDatabase;

public class DatabaseConfigurationTests
{
    [Fact]
    public void ConfigureDependencies_WithMissingSettings_ThrowsClearError()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () =>
            {
                services.ConfigureDependencies(configuration);
            });

        Assert.Contains("Database:Host", exception.Message);
        Assert.Contains("Database:Password", exception.Message);
    }

    [Fact]
    public void ConfigureDependencies_WithSettings_ConfiguresContextFactory()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Database:Host"] = "mysql",
            ["Database:Port"] = "3306",
            ["Database:Name"] = "expensewise",
            ["Database:Username"] = "expense",
            ["Database:Password"] = "local;password=part"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        services.ConfigureDependencies(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CoreContext>>();
        using var context = contextFactory.CreateDbContext();

        var connectionString = new MySqlConnectionStringBuilder(
            context.Database.GetDbConnection().ConnectionString);
        Assert.Equal("mysql", connectionString.Server);
        Assert.Equal(3306u, connectionString.Port);
        Assert.Equal("expensewise", connectionString.Database);
        Assert.Equal("expense", connectionString.UserID);
        Assert.Equal("local;password=part", connectionString.Password);
    }

    [Fact]
    public void CoreContextModel_MapsSettledColumnFromInitialMigration()
    {
        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseMySQL("Server=localhost;Database=expensewise;User ID=expense;Password=placeholder")
            .Options;
        using var context = new CoreContext(options);

        var entityType = context.Model.FindEntityType(typeof(ExpenseEntity))!;
        var column = entityType.FindProperty("Settled")!
            .GetColumnName(StoreObjectIdentifier.Table("expenses", null));

        Assert.Equal("settled", column);
    }
}
