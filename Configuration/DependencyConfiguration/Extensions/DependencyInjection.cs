using Microsoft.Extensions.DependencyInjection;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using OpenAPI;

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

        public static IServiceCollection ConfigureDependencies(this IServiceCollection serviceCollection)
        {

            #region Database Ports

            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IGroupRepository, GroupRepository>();
            serviceCollection.AddScoped<IExpenseRepository, ExpenseRepository>();
            serviceCollection.AddScoped<IPasswordRepository, PasswordRepository>();

            #endregion

            return serviceCollection;
        }

        public static IServiceCollection ConfigureExpenseWiseApi(this IServiceCollection serviceCollection)
        {
            #region OpenAPI Client
            serviceCollection.AddTransient<IExpenseWiseClient>(x =>
            {
                return new ExpenseWiseClient(new HttpClient());
            });
            #endregion

            return serviceCollection;
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
