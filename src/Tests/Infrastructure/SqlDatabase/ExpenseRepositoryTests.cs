using System;
using Application.UseCases.Ports;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure.SqlDatabase
{
    public class ExpenseRepositoryTests
    {
        private static DbContextOptions<CoreContext> CreateInMemoryOptions(string databaseName)
            => new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

        [Fact]
        public async Task CreateWithSharesAsync_PersistsSharesAndReturnsExistingExpenseForReusedKey()
        {
            var options = CreateInMemoryOptions(nameof(CreateWithSharesAsync_PersistsSharesAndReturnsExistingExpenseForReusedKey));
            var key = Guid.NewGuid();
            await SeedExpenseRelations(options);
            var repository = new ExpenseRepository(() => new CoreContext(options));
            var expense = BuildExpense(key);

            var created = await repository.CreateWithSharesAsync(expense, CancellationToken.None);

            await using var verifyContext = new CoreContext(options);
            Assert.True(created.Created);
            Assert.Equal(1, await verifyContext.Expenses.CountAsync());
            var storedShare = await verifyContext.ExpenseShares.SingleAsync();
            Assert.Equal(created.Expense.Id, storedShare.ExpenseId);
            Assert.Equal(2, storedShare.UserId);
            Assert.Equal(10m, storedShare.Amount);

            expense.Amount = 11m;
            var existing = await repository.CreateWithSharesAsync(expense, CancellationToken.None);
            Assert.False(existing.Created);
            Assert.Equal(created.Expense.Id, existing.Expense.Id);
            Assert.Equal(10m, existing.Expense.Amount);
            Assert.Equal(5, existing.Expense.BelongsTo!.Id);
            Assert.Equal(1, existing.Expense.CreatedBy!.Id);
            Assert.Equal(1, existing.Expense.Payer!.Id);
            Assert.Equal((2, 10m), existing.Expense.Shares.Select(share => (share.Participant.Id, share.Amount)).Single());
            Assert.Equal(1, await verifyContext.Expenses.CountAsync());
        }

        [Fact]
        public async Task VoidAsync_MarksExpenseVoidedWithoutDeletingShares()
        {
            var options = CreateInMemoryOptions(nameof(VoidAsync_MarksExpenseVoidedWithoutDeletingShares));
            var key = Guid.NewGuid();
            await SeedExpenseRelations(options);
            var repository = new ExpenseRepository(() => new CoreContext(options));
            var created = await repository.CreateWithSharesAsync(BuildExpense(key), CancellationToken.None);

            var outcome = await repository.VoidAsync(key, 5, CancellationToken.None);

            Assert.False(outcome.BlockedByRepayments);
            Assert.NotNull(outcome.Expense);
            Assert.True(outcome.Expense!.Voided);
            await using var verifyContext = new CoreContext(options);
            Assert.Equal(1, await verifyContext.ExpenseShares.CountAsync());
            Assert.True((await verifyContext.Expenses.SingleAsync()).Voided);
        }

        [Fact]
        public async Task CreateWithSharesAsync_WhenParticipantIsNoLongerMember_ThrowsAndStoresNothing()
        {
            var options = CreateInMemoryOptions(nameof(CreateWithSharesAsync_WhenParticipantIsNoLongerMember_ThrowsAndStoresNothing));
            await SeedExpenseRelations(options);
            await using (var context = new CoreContext(options))
            {
                context.MemberOf.RemoveRange(context.MemberOf.Where(membership => membership.UserId == 2));
                await context.SaveChangesAsync();
            }

            var repository = new ExpenseRepository(() => new CoreContext(options));

            await Assert.ThrowsAsync<GroupMembershipConflictException>(
                () => repository.CreateWithSharesAsync(BuildExpense(Guid.NewGuid()), CancellationToken.None));

            await using var verifyContext = new CoreContext(options);
            Assert.Equal(0, await verifyContext.Expenses.CountAsync());
        }

        private static Expense BuildExpense(Guid key) => new()
        {
            Description = "Shared expense",
            Currency = "USD",
            Amount = 10m,
            UniqueKey = key,
            CreatedBy = new User { Id = 1, UniqueKey = Guid.NewGuid() },
            Payer = new User { Id = 1, UniqueKey = Guid.NewGuid() },
            BelongsTo = new Group { Id = 5, UniqueKey = Guid.NewGuid() },
            Shares =
            [
                new ExpenseShare
                {
                    Participant = new User { Id = 2, UniqueKey = Guid.NewGuid() },
                    Amount = 10m
                }
            ]
        };

        private static async Task SeedExpenseRelations(DbContextOptions<CoreContext> options)
        {
            await using var context = new CoreContext(options);
            context.Users.AddRange(
                new UserEntity { Id = 1, FirstName = "Owner", LastName = "One", Email = "owner@example.test", Phone = "100", CountryCode = "+1", UniqueKey = Guid.NewGuid() },
                new UserEntity { Id = 2, FirstName = "Member", LastName = "Two", Email = "member@example.test", Phone = "200", CountryCode = "+1", UniqueKey = Guid.NewGuid() });
            context.Groups.Add(new GroupEntity { Id = 5, OwnerId = 1, Name = "Group", UniqueKey = Guid.NewGuid() });
            context.MemberOf.AddRange(
                new MemberOfEntity { UserId = 1, GroupId = 5, Active = true },
                new MemberOfEntity { UserId = 2, GroupId = 5, Active = true });
            await context.SaveChangesAsync();
        }
    }
}
