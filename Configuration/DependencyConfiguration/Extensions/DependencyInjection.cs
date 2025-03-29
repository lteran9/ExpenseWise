using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseWise.DependencyConfiguration
{
   public static class DependencyInjection
   {
      public static void ConfigureDependencies(this IServiceCollection serviceCollection)
      {
         serviceCollection.AddTransient<ISqlDatabase<UserEntity>, UserRepository>();
         serviceCollection.AddTransient<ISqlDatabase<GroupEntity>, GroupRepository>();
         serviceCollection.AddTransient<IRepository, RepositoryAdapter>();
      }
   }
}