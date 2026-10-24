using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Tests.Infrastructure.SqlDatabase
{
    public class RepaymentRepositoryMySqlIntegrationTests
    {
        private const string ConnectionStringEnvironmentVariable = "EXPENSEWISE_TEST_MYSQL_CONNECTION";
        private const string PasswordEnvironmentVariable = "Database__Password";

        [MySqlIntegrationFact]
        public async Task RecordAsync_ConcurrentSameKeyRequests_StoresOneRepayment()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var repository = new RepaymentRepository(() => new CoreContext(options));
                var key = Guid.NewGuid();

                var results = await Task.WhenAll(
                    repository.RecordAsync(BuildRepayment(key, seed, 5m), ValidateFor(seed, 5m), CancellationToken.None),
                    repository.RecordAsync(BuildRepayment(key, seed, 5m), ValidateFor(seed, 5m), CancellationToken.None));

                Assert.Single(results, result => result.Created);
                Assert.All(results, result => Assert.Null(result.RejectionReason));
                Assert.Equal(results[0].Repayment!.Id, results[1].Repayment!.Id);

                await using var context = new CoreContext(options);
                Assert.Equal(1, await context.Repayments.CountAsync(repayment => repayment.UniqueKey == key));
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task RecordAsync_AmountExceedingCurrentDebt_RejectsWithoutWriting()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var repository = new RepaymentRepository(() => new CoreContext(options));

                var first = await repository.RecordAsync(
                    BuildRepayment(Guid.NewGuid(), seed, 3m), ValidateFor(seed, 3m), CancellationToken.None);
                Assert.True(first.Created);

                var overpayment = await repository.RecordAsync(
                    BuildRepayment(Guid.NewGuid(), seed, 3m), ValidateFor(seed, 3m), CancellationToken.None);

                Assert.False(overpayment.Created);
                Assert.Null(overpayment.Repayment);
                Assert.Contains("exceeds", overpayment.RejectionReason);

                await using var context = new CoreContext(options);
                Assert.Equal(1, await context.Repayments.CountAsync(repayment => repayment.GroupId == seed.GroupId));
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task VoidAsync_AfterRepaymentRecorded_RejectsAndKeepsExpenseActive()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var repayments = new RepaymentRepository(() => new CoreContext(options));
                var expenses = new ExpenseRepository(() => new CoreContext(options));
                var recorded = await repayments.RecordAsync(
                    BuildRepayment(Guid.NewGuid(), seed, 2m), ValidateFor(seed, 2m), CancellationToken.None);
                Assert.True(recorded.Created);

                var outcome = await expenses.VoidAsync(seed.ExpenseKey, seed.GroupId, CancellationToken.None);

                Assert.True(outcome.BlockedByRepayments);
                Assert.Null(outcome.Expense);
                await using var context = new CoreContext(options);
                Assert.False(await context.Expenses
                    .Where(expense => expense.UniqueKey == seed.ExpenseKey)
                    .Select(expense => expense.Voided)
                    .SingleAsync());
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task VoidAsync_ContendedWithRepayment_OperationThatAcquiresGroupLockFirstWins()
        {
            foreach (var repaymentWinsFirst in new[] { false, true })
            {
                var seedOptions = CreateOptions();
                var seed = await SeedGroupWithDebt(seedOptions);
                var coordinator = new GroupLockContentionCoordinator();
                var options = new DbContextOptionsBuilder<CoreContext>(seedOptions)
                    .AddInterceptors(coordinator)
                    .Options;

                try
                {
                    var repayments = new RepaymentRepository(() => new CoreContext(options));
                    var expenses = new ExpenseRepository(() => new CoreContext(options));
                    Task<VoidExpenseOutcome> voidTask;
                    Task<RecordRepaymentOutcome> repaymentTask;

                    if (repaymentWinsFirst)
                    {
                        repaymentTask = repayments.RecordAsync(
                            BuildRepayment(Guid.NewGuid(), seed, 5m), ValidateFor(seed, 5m), CancellationToken.None);
                        await coordinator.FirstLockAcquired.WaitAsync(TimeSpan.FromSeconds(15));
                        voidTask = expenses.VoidAsync(seed.ExpenseKey, seed.GroupId, CancellationToken.None);
                    }
                    else
                    {
                        voidTask = expenses.VoidAsync(seed.ExpenseKey, seed.GroupId, CancellationToken.None);
                        await coordinator.FirstLockAcquired.WaitAsync(TimeSpan.FromSeconds(15));
                        repaymentTask = repayments.RecordAsync(
                            BuildRepayment(Guid.NewGuid(), seed, 5m), ValidateFor(seed, 5m), CancellationToken.None);
                    }

                    await coordinator.CompetingLockAttempted.WaitAsync(TimeSpan.FromSeconds(15));
                    coordinator.ReleaseFirstLock();
                    await Task.WhenAll(voidTask, repaymentTask);

                    await using var context = new CoreContext(options);
                    var voided = await context.Expenses
                        .Where(expense => expense.UniqueKey == seed.ExpenseKey)
                        .Select(expense => expense.Voided)
                        .SingleAsync();
                    var repaymentCount = await context.Repayments.CountAsync(repayment => repayment.GroupId == seed.GroupId);

                    var voidOutcome = await voidTask;
                    var repaymentOutcome = await repaymentTask;
                    if (repaymentWinsFirst)
                    {
                        Assert.True(repaymentOutcome.Created);
                        Assert.True(voidOutcome.BlockedByRepayments);
                        Assert.Null(voidOutcome.Expense);
                        Assert.False(voided);
                        Assert.Equal(1, repaymentCount);
                    }
                    else
                    {
                        Assert.False(repaymentOutcome.Created);
                        Assert.False(voidOutcome.BlockedByRepayments);
                        Assert.NotNull(voidOutcome.Expense);
                        Assert.True(voided);
                        Assert.Equal(0, repaymentCount);
                    }
                }
                finally
                {
                    coordinator.ReleaseFirstLock();
                    await Cleanup(seedOptions, seed);
                }
            }
        }

        private static Func<GroupLedger, string?> ValidateFor(SeedData seed, decimal amount) =>
            ledger => ledger.ValidateRepayment(seed.ParticipantKey, seed.OwnerKey, amount);

        private static Repayment BuildRepayment(Guid key, SeedData seed, decimal amount) => new()
        {
            UniqueKey = key,
            Currency = "USD",
            Amount = amount,
            OccurredAt = DateTime.UtcNow,
            BelongsTo = new Group { Id = seed.GroupId, UniqueKey = seed.GroupKey },
            Sender = new User { Id = seed.ParticipantId, UniqueKey = seed.ParticipantKey },
            Recipient = new User { Id = seed.OwnerId, UniqueKey = seed.OwnerKey },
            RecordedBy = new User { Id = seed.OwnerId, UniqueKey = seed.OwnerKey }
        };

        private static DbContextOptions<CoreContext> CreateOptions()
        {
            var connectionString = new MySqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable))
            {
                Password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable)
            };

            return new DbContextOptionsBuilder<CoreContext>()
                .UseMySQL(connectionString.ConnectionString)
                .Options;
        }

        // Owner paid 10 for a 10-unit expense split between participant and other, so participant owes owner 5.
        private static async Task<SeedData> SeedGroupWithDebt(DbContextOptions<CoreContext> options)
        {
            var now = DateTime.UtcNow;
            var owner = BuildUser(now);
            var participant = BuildUser(now);
            var otherParticipant = BuildUser(now);

            await using var context = new CoreContext(options);
            var group = new GroupEntity
            {
                Owner = owner,
                Name = "Repayment integration test group",
                UniqueKey = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now,
                Membership =
                [
                    new MemberOfEntity { User = owner, Active = true, CreatedAt = now, UpdatedAt = now },
                    new MemberOfEntity { User = participant, Active = true, CreatedAt = now, UpdatedAt = now },
                    new MemberOfEntity { User = otherParticipant, Active = true, CreatedAt = now, UpdatedAt = now }
                ]
            };
            context.Users.AddRange(owner, participant, otherParticipant);
            context.Groups.Add(group);
            await context.SaveChangesAsync();

            var seed = new SeedData(group.Id, group.UniqueKey, owner.Id, owner.UniqueKey,
                participant.Id, participant.UniqueKey, otherParticipant.Id, otherParticipant.UniqueKey, Guid.NewGuid());

            var expenseRepository = new ExpenseRepository(() => new CoreContext(options));
            await expenseRepository.CreateWithSharesAsync(new Expense
            {
                Description = "Repayment integration expense",
                Currency = "USD",
                Amount = 10m,
                UniqueKey = seed.ExpenseKey,
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
            }, CancellationToken.None);

            return seed;
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

        private static async Task Cleanup(DbContextOptions<CoreContext> options, SeedData seed)
        {
            await using var context = new CoreContext(options);
            await context.Repayments.Where(repayment => repayment.GroupId == seed.GroupId).ExecuteDeleteAsync();
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
            Guid OtherParticipantKey,
            Guid ExpenseKey);

        private sealed class GroupLockContentionCoordinator : DbCommandInterceptor
        {
            private readonly TaskCompletionSource _firstLockAcquired =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _competingLockAttempted =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _releaseFirstLock =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _lockAttemptCount;

            public Task FirstLockAcquired => _firstLockAcquired.Task;
            public Task CompetingLockAttempted => _competingLockAttempted.Task;

            public void ReleaseFirstLock() => _releaseFirstLock.TrySetResult();

            public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                if (IsGroupLock(command) && Interlocked.Increment(ref _lockAttemptCount) == 2)
                {
                    _competingLockAttempted.TrySetResult();
                }

                return ValueTask.FromResult(result);
            }

            public override async ValueTask<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                if (IsGroupLock(command) && _firstLockAcquired.TrySetResult())
                {
                    await _releaseFirstLock.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
                }

                return result;
            }

            private static bool IsGroupLock(DbCommand command) =>
                command.CommandText.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
