using System;
using ExpenseWise.Infrastructure.Sql;
using Microsoft.EntityFrameworkCore;

namespace ExpenseWise.SqlDatabase
{
   public class CoreContext : DbContext
   {
      public DbSet<User> Users { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseMySQL("server=localhost;port=3316;database=expensewise;user=web_user;password=password");
      }
   }
}