using Application.UseCases.Ports;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

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
        public async Task VoidAsync_AfterRepaymentRecorded_IsBlockedWithoutWriting()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var repaymentRepository = new RepaymentRepository(() => new CoreContext(options));
                var recorded = await repaymentRepository.RecordAsync(
                    BuildRepayment(Guid.NewGuid(), seed, 1m), ValidateFor(seed, 1m), CancellationToken.None);
                Assert.True(recorded.Created);

                var expenseRepository = new ExpenseRepository(() => new CoreContext(options));
                var outcome = await expenseRepository.VoidAsync(seed.ExpenseKey, seed.GroupId, CancellationToken.None);

                Assert.True(outcome.BlockedByRepayments);
                Assert.Null(outcome.Expense);
                await using var context = new CoreContext(options);
                Assert.False((await context.Expenses.SingleAsync(expense => expense.UniqueKey == seed.ExpenseKey)).Voided);
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task VoidAsync_RacingRepaymentForWholeDebt_AppliesExactlyOneChange()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var expenseRepository = new ExpenseRepository(() => new CoreContext(options));
                var repaymentRepository = new RepaymentRepository(() => new CoreContext(options));

                var voidTask = expenseRepository.VoidAsync(seed.ExpenseKey, seed.GroupId, CancellationToken.None);
                var repaymentTask = repaymentRepository.RecordAsync(
                    BuildRepayment(Guid.NewGuid(), seed, 5m), ValidateFor(seed, 5m), CancellationToken.None);
                await Task.WhenAll(voidTask, repaymentTask);

                var voided = voidTask.Result.Expense?.Voided == true;
                var repaid = repaymentTask.Result.Created;
                Assert.True(voided ^ repaid, $"Expected exactly one change; voided={voided}, repaid={repaid}.");

                await using var context = new CoreContext(options);
                var storedVoided = (await context.Expenses.SingleAsync(expense => expense.UniqueKey == seed.ExpenseKey)).Voided;
                var storedRepayments = await context.Repayments.CountAsync(repayment => repayment.GroupId == seed.GroupId);
                Assert.Equal(voided, storedVoided);
                Assert.Equal(repaid ? 1 : 0, storedRepayments);
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task RecordAsync_SmallestAmount_PersistsExactDecimal()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var repository = new RepaymentRepository(() => new CoreContext(options));
                var key = Guid.NewGuid();

                var result = await repository.RecordAsync(
                    BuildRepayment(key, seed, 0.01m), ValidateFor(seed, 0.01m), CancellationToken.None);

                Assert.True(result.Created);
                await using var context = new CoreContext(options);
                Assert.Equal(0.01m, (await context.Repayments.SingleAsync(repayment => repayment.UniqueKey == key)).Amount);
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task Repayments_DuplicateUniqueKeyOrUnknownSender_AreRejectedByConstraints()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                var key = Guid.NewGuid();
                await using (var context = new CoreContext(options))
                {
                    context.Repayments.Add(BuildRepaymentEntity(key, seed, seed.ParticipantId));
                    await context.SaveChangesAsync();
                }

                await using (var context = new CoreContext(options))
                {
                    context.Repayments.Add(BuildRepaymentEntity(key, seed, seed.ParticipantId));
                    var duplicate = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    Assert.Equal(1062, Assert.IsType<MySqlException>(duplicate.InnerException).Number);
                }

                await using (var context = new CoreContext(options))
                {
                    context.Repayments.Add(BuildRepaymentEntity(Guid.NewGuid(), seed, int.MaxValue));
                    var unknownSender = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    Assert.Equal(1452, Assert.IsType<MySqlException>(unknownSender.InnerException).Number);
                }

                await using var verifyContext = new CoreContext(options);
                Assert.Equal(1, await verifyContext.Repayments.CountAsync(repayment => repayment.GroupId == seed.GroupId));
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        [MySqlIntegrationFact]
        public async Task Membership_DuplicateUserAndGroup_IsRejectedByUniqueConstraint()
        {
            var options = CreateOptions();
            var seed = await SeedGroupWithDebt(options);

            try
            {
                await using var context = new CoreContext(options);
                var now = DateTime.UtcNow;
                context.Add(new MemberOfEntity
                {
                    UserId = seed.ParticipantId,
                    GroupId = seed.GroupId,
                    Active = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                Assert.Equal(1062, Assert.IsType<MySqlException>(exception.InnerException).Number);
            }
            finally
            {
                await Cleanup(options, seed);
            }
        }

        private static RepaymentEntity BuildRepaymentEntity(Guid key, SeedData seed, int senderId)
        {
            var now = DateTime.UtcNow;
            return new RepaymentEntity
            {
                UniqueKey = key,
                GroupId = seed.GroupId,
                SenderUserId = senderId,
                RecipientUserId = seed.OwnerId,
                RecordedByUserId = seed.OwnerId,
                Amount = 1m,
                Currency = "USD",
                OccurredAt = now,
                CreatedAt = now
            };
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
    }
}
