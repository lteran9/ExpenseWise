using System;
using Microsoft.Extensions.DependencyInjection;
using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;

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
         serviceCollection.AddSingleton<RepositoryAdapter>();

         serviceCollection.AddScoped<IDatabasePort<User>>(x =>
         {
            return x.GetService<RepositoryAdapter>()!;
         });
         serviceCollection.AddScoped<IDatabasePort<Group>>(x =>
         {
            return x.GetService<RepositoryAdapter>()!;
         });
         serviceCollection.AddScoped<IDatabasePort<MemberOf>>(x =>
         {
            return x.GetService<RepositoryAdapter>()!;
         });
         serviceCollection.AddScoped<IDatabasePort<Expense>>(x =>
         {
            return x.GetService<RepositoryAdapter>()!;
         });
         serviceCollection.AddScoped<IDatabasePort<Password>>(x =>
         {
            return x.GetService<RepositoryAdapter>()!;
         });

         return serviceCollection;
      }

      public static IServiceCollection ConfigureSession(this IServiceCollection serviceCollection, long expiration = 20)
      {
         serviceCollection.AddSession(options =>
         {
            options.IdleTimeout = TimeSpan.FromMinutes(expiration);
         });

         return serviceCollection;
      }
   }
}