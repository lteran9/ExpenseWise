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
        public DbSet<ExpenseShareEntity> ExpenseShares { get; set; }
        public DbSet<PasswordEntity> Passwords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new InvalidOperationException(
                    "CoreContext must be configured through dependency injection with database settings.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpenseEntity>()
                .HasOne(expense => expense.Payer)
                .WithMany()
                .HasForeignKey(expense => expense.PayerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExpenseShareEntity>()
                .HasOne(share => share.Participant)
                .WithMany()
                .HasForeignKey(share => share.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
