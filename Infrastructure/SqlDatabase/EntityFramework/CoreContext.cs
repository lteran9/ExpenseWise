using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    public class CoreContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<MemberOfEntity> MemberOf { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<SplitEntity> Splits { get; set; }
        public DbSet<PasswordEntity> Passwords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseMySQL("server=localhost;port=3316;database=expensewise;user=web_user;password=password");
            optionsBuilder.UseMySQL("server=expensewise-mysql-1;port=3306;database=expensewise;user=web_user;password=password");
        }
    }
}
