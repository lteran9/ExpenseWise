using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   public class CoreContext : DbContext
   {
      public DbSet<UserEntity> Users { get; set; }
      public DbSet<GroupEntity> Groups { get; set; }
      public DbSet<MemberOfEntity> MemberOf { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseMySQL("server=localhost;port=3316;database=expensewise;user=web_user;password=password");
      }
   }
}