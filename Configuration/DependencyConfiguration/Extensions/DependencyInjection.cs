using System;
using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.Extensions.DependencyInjection;

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

         return serviceCollection;
      }
   }
}