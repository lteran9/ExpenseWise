using ExpenseWise.DependencyConfiguration;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Tests.Infrastructure.SqlDatabase
{

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
        public void ConfigureDependencies_WithSslMode_ConfiguresMySqlDataConnection()
        {
            var configuration = CreateConfiguration("VerifyFull");
            var services = new ServiceCollection();
            services.ConfigureDependencies(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CoreContext>>();
            using var context = contextFactory.CreateDbContext();

            var connectionString = new MySqlConnectionStringBuilder(
                context.Database.GetDbConnection().ConnectionString);

            Assert.Equal(MySqlSslMode.VerifyFull, connectionString.SslMode);
        }

        [Fact]
        public void ConfigureDependencies_WithUnsupportedSslMode_ThrowsClearError()
        {
            var configuration = CreateConfiguration("NotARealMode");
            var services = new ServiceCollection();

            var exception = Assert.Throws<InvalidOperationException>(
                () => services.ConfigureDependencies(configuration));

            Assert.Contains("Database:SslMode", exception.Message);
        }

        [Fact]
        public void ConfigureDependencies_WithoutSslMode_PreservesMySqlDefault()
        {
            var configuration = CreateConfiguration();
            var services = new ServiceCollection();
            services.ConfigureDependencies(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CoreContext>>();
            using var context = contextFactory.CreateDbContext();
            var connectionString = new MySqlConnectionStringBuilder(
                context.Database.GetDbConnection().ConnectionString);

            Assert.Equal(new MySqlConnectionStringBuilder().SslMode, connectionString.SslMode);
        }

        [Fact]
        public void CoreContextModel_MapsExpenseSharesAndProtectsHistoryReferences()
        {
            var options = new DbContextOptionsBuilder<CoreContext>()
                .UseMySQL("Server=localhost;Database=expensewise;User ID=expense;Password=placeholder")
                .Options;
            using var context = new CoreContext(options);

            var expenseType = context.Model.FindEntityType(typeof(ExpenseEntity))!;
            var voidedColumn = expenseType.FindProperty(nameof(ExpenseEntity.Voided))!
                .GetColumnName(StoreObjectIdentifier.Table("expenses", null));
            var payerForeignKey = expenseType.GetForeignKeys()
                .Single(foreignKey => foreignKey.Properties.Any(property => property.Name == nameof(ExpenseEntity.PayerUserId)));
            var shareType = context.Model.FindEntityType(typeof(ExpenseShareEntity))!;
            var participantForeignKey = shareType.GetForeignKeys()
                .Single(foreignKey => foreignKey.Properties.Any(property => property.Name == nameof(ExpenseShareEntity.UserId)));

            Assert.Equal("voided", voidedColumn);
            Assert.Equal("expense_shares", shareType.GetTableName());
            Assert.Equal(DeleteBehavior.Restrict, payerForeignKey.DeleteBehavior);
            Assert.Equal(DeleteBehavior.Restrict, participantForeignKey.DeleteBehavior);
            Assert.Contains(shareType.GetIndexes(), index =>
                index.IsUnique &&
                index.Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(ExpenseShareEntity.ExpenseId), nameof(ExpenseShareEntity.UserId)]));
        }

        private static IConfiguration CreateConfiguration(string? sslMode = null)
        {
            var settings = new Dictionary<string, string?>
            {
                ["Database:Host"] = "mysql",
                ["Database:Port"] = "3306",
                ["Database:Name"] = "expensewise",
                ["Database:Username"] = "expense",
                ["Database:Password"] = "test-password"
            };
            if (sslMode is not null)
            {
                settings["Database:SslMode"] = sslMode;
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}
