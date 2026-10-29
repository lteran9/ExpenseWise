using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace ExpenseWise.DependencyConfiguration
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterUseCases(this IServiceCollection serviceCollection)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
            }

            return serviceCollection;
        }

        public static IServiceCollection ConfigureDependencies(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var connectionString = BuildConnectionString(configuration);

            serviceCollection.AddDbContextFactory<CoreContext>(options =>
                options.UseMySQL(connectionString));

            serviceCollection.AddScoped<Func<CoreContext>>(services =>
                () => services.GetRequiredService<IDbContextFactory<CoreContext>>().CreateDbContext());
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IGroupRepository, GroupRepository>();
            serviceCollection.AddScoped<IExpenseRepository, ExpenseRepository>();
            serviceCollection.AddScoped<IRepaymentRepository, RepaymentRepository>();
            serviceCollection.AddScoped<IPasswordRepository, PasswordRepository>();

            return serviceCollection;
        }

        private static string BuildConnectionString(IConfiguration configuration)
        {
            var requiredSettings = new[]
            {
                "Database:Host",
                "Database:Port",
                "Database:Name",
                "Database:Username",
                "Database:Password"
            };
            var missingSettings = requiredSettings
                .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
                .ToArray();

            if (missingSettings.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Missing required database configuration: {string.Join(", ", missingSettings)}.");
            }

            if (!uint.TryParse(configuration["Database:Port"], out var port) || port == 0)
            {
                throw new InvalidOperationException("Database:Port must be a positive integer.");
            }

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = configuration["Database:Host"]!,
                Port = port,
                Database = configuration["Database:Name"]!,
                UserID = configuration["Database:Username"]!,
                Password = configuration["Database:Password"]!
            };

            return connectionStringBuilder.ConnectionString;
        }

        public static IServiceCollection ConfigureSession(this IServiceCollection serviceCollection, long expiration = 20)
        {
            serviceCollection.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(expiration);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return serviceCollection;
        }
    }
}
