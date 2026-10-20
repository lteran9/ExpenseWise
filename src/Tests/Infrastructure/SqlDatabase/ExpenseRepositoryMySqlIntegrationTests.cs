using System.Data.Common;
using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MySql.Data.MySqlClient;

namespace Tests.Infrastructure.SqlDatabase
{
    public class ExpenseRepositoryMySqlIntegrationTests
    {
        private const string ConnectionStringEnvironmentVariable = "EXPENSEWISE_TEST_MYSQL_CONNECTION";
        private const string PasswordEnvironmentVariable = "Database__Password";

        [MySqlIntegrationFact]
        public async Task CreateWithSharesAsync_ConcurrentSameKeyRequests_ReturnOneCompleteExpense()
        {
            var interceptor = new InitialLookupBarrier();
            var options = CreateOptions(interceptor);
            var seed = await SeedRelations(options);

            try
            {
                var repository = new ExpenseRepository(() => new CoreContext(options));
                var key = Guid.NewGuid();

                var results = await Task.WhenAll(
                    repository.CreateWithSharesAsync(BuildExpense(key, seed), CancellationToken.None),
                    repository.CreateWithSharesAsync(BuildExpense(key, seed), CancellationToken.None));

                Assert.Single(results, result => result.Created);
                Assert.Equal(results[0].Expense.Id, results[1].Expense.Id);
                Assert.All(results, result => Assert.Equal(2, result.Expense.Shares.Count));

                await using var context = new CoreContext(options);
                var storedExpense = await context.Expenses.SingleAsync(expense => expense.UniqueKey == key);
                var shares = await context.ExpenseShares
                    .Where(share => share.ExpenseId == storedExpense.Id)
                    .OrderBy(share => share.UserId)
                    .ToListAsync();

                Assert.Equal(2, shares.Count);
                Assert.Equal(new[] { seed.ParticipantId, seed.OtherParticipantId }, shares.Select(share => share.UserId));
                Assert.Equal(1, await context.Expenses.CountAsync(expense => expense.UniqueKey == key));
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task CreateWithSharesAsync_WhenShareInsertFails_RollsBackExpense()
        {
            var options = CreateOptions();
            var seed = await SeedRelations(options);

            try
            {
                int missingUserId;
                await using (var context = new CoreContext(options))
                {
                    missingUserId = -1;
                    while (await context.Users.AnyAsync(user => user.Id == missingUserId))
                    {
                        missingUserId--;
                    }
                }

                var expense = BuildExpense(Guid.NewGuid(), seed);
                expense.Shares =
                [
                    new ExpenseShare
                    {
                        Participant = new User { Id = missingUserId, UniqueKey = Guid.NewGuid() },
                        Amount = 10m
                    }
                ];
                var repository = new ExpenseRepository(() => new CoreContext(options));

                await Assert.ThrowsAsync<DbUpdateException>(
                    () => repository.CreateWithSharesAsync(expense, CancellationToken.None));

                await using var verifyContext = new CoreContext(options);
                Assert.Empty(await verifyContext.Expenses.Where(item => item.UniqueKey == expense.UniqueKey).ToListAsync());
                var groupExpenseIds = verifyContext.Expenses
                    .Where(item => item.GroupId == seed.GroupId)
                    .Select(item => item.Id);
                Assert.Empty(await verifyContext.ExpenseShares
                    .Where(share => groupExpenseIds.Contains(share.ExpenseId))
                    .ToListAsync());
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        private static DbContextOptions<CoreContext> CreateOptions(params IInterceptor[] interceptors)
        {
            var connectionString = new MySqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable))
            {
                Password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable)
            };

            return new DbContextOptionsBuilder<CoreContext>()
                .UseMySQL(connectionString.ConnectionString)
                .AddInterceptors(interceptors)
                .Options;
        }

        private static async Task<SeedData> SeedRelations(DbContextOptions<CoreContext> options)
        {
            var now = DateTime.UtcNow;
            var owner = BuildUser(now);
            var participant = BuildUser(now);
            var otherParticipant = BuildUser(now);

            await using var context = new CoreContext(options);
            var group = new GroupEntity
            {
                Owner = owner,
                Name = "Integration test group",
                UniqueKey = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now
            };
            context.Users.AddRange(owner, participant, otherParticipant);
            context.Groups.Add(group);
            await context.SaveChangesAsync();

            return new SeedData(group.Id, group.UniqueKey, owner.Id, owner.UniqueKey, participant.Id,
                participant.UniqueKey, otherParticipant.Id, otherParticipant.UniqueKey);
        }

        private static UserEntity BuildUser(DateTime now)
        {
            var key = Guid.NewGuid();
            return new UserEntity
            {
                FirstName = "Integration",
                LastName = "Test",
                Email = $"{key:N}@example.test",
                Phone = key.ToString("N")[..16],
                CountryCode = "+1",
                UniqueKey = key,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        private static Expense BuildExpense(Guid key, SeedData seed) => new()
        {
            Description = "Concurrent integration expense",
            Currency = "USD",
            Amount = 10m,
            UniqueKey = key,
            CreatedBy = new User { Id = seed.OwnerId, UniqueKey = seed.OwnerKey },
            Payer = new User { Id = seed.OwnerId, UniqueKey = seed.OwnerKey },
            BelongsTo = new Group { Id = seed.GroupId, UniqueKey = seed.GroupKey },
            Shares =
            [
                new ExpenseShare
                {
                    Participant = new User { Id = seed.ParticipantId, UniqueKey = seed.ParticipantKey },
                    Amount = 5m
                },
                new ExpenseShare
                {
                    Participant = new User { Id = seed.OtherParticipantId, UniqueKey = seed.OtherParticipantKey },
                    Amount = 5m
                }
            ]
        };

        private static async Task Cleanup(DbContextOptions<CoreContext> options, SeedData seed)
        {
            await using var context = new CoreContext(options);
            await context.Expenses.Where(expense => expense.GroupId == seed.GroupId).ExecuteDeleteAsync();
            await context.Groups.Where(group => group.Id == seed.GroupId).ExecuteDeleteAsync();
            await context.Users
                .Where(user => user.Id == seed.OwnerId ||
                               user.Id == seed.ParticipantId ||
                               user.Id == seed.OtherParticipantId)
                .ExecuteDeleteAsync();
        }

        private sealed record SeedData(
            int GroupId,
            Guid GroupKey,
            int OwnerId,
            Guid OwnerKey,
            int ParticipantId,
            Guid ParticipantKey,
            int OtherParticipantId,
            Guid OtherParticipantKey);

        private sealed class InitialLookupBarrier : DbCommandInterceptor
        {
            private readonly TaskCompletionSource _lookupsReachedBarrier =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _lookupCount;

            public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                if (command.CommandText.Contains("expenses", StringComparison.OrdinalIgnoreCase) &&
                    command.CommandText.Contains("unique_key", StringComparison.OrdinalIgnoreCase) &&
                    Interlocked.Increment(ref _lookupCount) <= 2)
                {
                    if (Volatile.Read(ref _lookupCount) == 2)
                    {
                        _lookupsReachedBarrier.TrySetResult();
                    }

                    await _lookupsReachedBarrier.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
                }

                return result;
            }
        }
    }
}
