using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    public class CoreContext : DbContext
    {
        public CoreContext(DbContextOptions<CoreContext> options)
            : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<MemberOfEntity> MemberOf { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<SplitEntity> Splits { get; set; }
        public DbSet<PasswordEntity> Passwords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Preserve the initial migration's legacy column without mapping it into the domain model.
            modelBuilder.Entity<ExpenseEntity>()
                .Property<bool>("Settled")
                .HasColumnName("settled");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new InvalidOperationException(
                    "CoreContext must be configured through dependency injection with database settings.");
            }
        }
    }
}
