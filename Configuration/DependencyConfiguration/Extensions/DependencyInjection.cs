using System;
using Microsoft.Extensions.DependencyInjection;
using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Application.UseCases;
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
            // serviceCollection.AddSingleton<IDatabasePort<User>>(repositoryAdapter);
            // serviceCollection.AddSingleton<IDatabasePort<Group>>(repositoryAdapter);
            // serviceCollection.AddSingleton<IDatabasePort<MemberOf>>(repositoryAdapter);
            // serviceCollection.AddSingleton<IDatabasePort<Expense>>(repositoryAdapter);
            // serviceCollection.AddSingleton<IDatabasePort<Password>>(repositoryAdapter);
            // serviceCollection.AddSingleton<IDatabasePort<Split>>(repositoryAdapter);

            #endregion

            #region Query Ports

            var queryAdapter = new QueryAdapter();

            serviceCollection.AddSingleton<IQueryPort<Group>>(queryAdapter);

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
