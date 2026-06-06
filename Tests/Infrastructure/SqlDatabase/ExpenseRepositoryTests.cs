using System;
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
        public async Task GivenNewExpense_WhenCreateAsync_ThenReturnsSavedExpense()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewExpense_WhenCreateAsync_ThenReturnsSavedExpense));
            var repository = new ExpenseRepository(() => new CoreContext(options));

            var expense = new Expense
            {
                Description = "Hotel",
                Currency = "USD",
                Amount = 240.50m
            };

            var created = await repository.CreateAsync(expense);

            Assert.NotNull(created);
            Assert.Equal("Hotel", created!.Description);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Expenses.FirstOrDefaultAsync(x => x.Description == "Hotel");
            Assert.NotNull(stored);
            Assert.Equal("Hotel", stored!.Description);
        }

        [Fact]
        public async Task GivenExistingExpense_WhenDeleteAsync_ThenRemovesExpense()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingExpense_WhenDeleteAsync_ThenRemovesExpense));

            await using (var context = new CoreContext(options))
            {
                context.Expenses.Add(new ExpenseEntity
                {
                    Id = 1,
                    Description = "Taxi",
                    Currency = "USD",
                    Amount = 42.75m,
                    UniqueKey = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var repository = new ExpenseRepository(() => new CoreContext(options));
            var deleted = await repository.DeleteAsync(new Expense { Id = 1 });

            Assert.NotNull(deleted);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Expenses.FindAsync(1);
            Assert.Null(stored);
        }

        [Fact]
        public async Task GivenNewSplit_WhenAddSplitAsync_ThenReturnsSavedSplit()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewSplit_WhenAddSplitAsync_ThenReturnsSavedSplit));
            var repository = new ExpenseRepository(() => new CoreContext(options));

            var split = new Split
            {
                User = new User { Id = 1 },
                Group = new Group { Id = 2 },
                Expense = new Expense { Id = 3 },
                Paid = false
            };

            var created = await repository.AddSplitAsync(split);

            Assert.NotNull(created);
            Assert.Equal(1, created!.User.Id);
            Assert.Equal(2, created.Group.Id);
            Assert.Equal(3, created.Expense.Id);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Splits.FirstOrDefaultAsync(x => x.UserId == 1 && x.GroupId == 2 && x.ExpenseId == 3);
            Assert.NotNull(stored);
        }
    }
}
