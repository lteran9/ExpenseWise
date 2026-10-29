using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{

    public class CreateExpenseTests
    {
        [Fact]
        public async Task CreateExpense_WithThreeParticipants_AllocatesRemainderInStableKeyOrder()
        {
            var first = Member(Guid.Parse("00000000-0000-0000-0000-000000000001"), 1);
            var second = Member(Guid.Parse("00000000-0000-0000-0000-000000000002"), 2);
            var third = Member(Guid.Parse("00000000-0000-0000-0000-000000000003"), 3);
            var groupKey = Guid.NewGuid();
            var requestKey = Guid.NewGuid();
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    Id = 12,
                    UniqueKey = groupKey,
                    Owner = first,
                    Members = [third, first, second]
                });
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.CreateWithSharesAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expense expense, CancellationToken _) =>
                {
                    expense.Id = 44;
                    return (expense, true);
                });
            var useCase = new CreateExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(new CreateExpenseRequest
            {
                ActorKey = first.UniqueKey,
                GroupKey = groupKey,
                IdempotencyKey = requestKey,
                Description = "Dinner",
                Currency = "USD",
                Amount = 10m,
                PayerKey = third.UniqueKey,
                ParticipantKeys = [third.UniqueKey, first.UniqueKey, second.UniqueKey]
            }, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(44, response.Result!.Id);
            Assert.Equal(requestKey, response.Result.UniqueKey);

            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.Is<Expense>(expense =>
                    expense.Payer!.UniqueKey == third.UniqueKey &&
                    expense.CreatedBy!.UniqueKey == first.UniqueKey &&
                    expense.Shares.Sum(share => share.Amount) == 10m &&
                    expense.Shares.Single(share => share.Participant.UniqueKey == first.UniqueKey).Amount == 3.34m &&
                    expense.Shares.Single(share => share.Participant.UniqueKey == second.UniqueKey).Amount == 3.33m &&
                    expense.Shares.Single(share => share.Participant.UniqueKey == third.UniqueKey).Amount == 3.33m),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateExpense_WithPayerOutsideParticipantSet_AllowsDifferentPayer()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var payer = Member(Guid.NewGuid(), 2);
            var participant = Member(Guid.NewGuid(), 3);
            var groupKey = Guid.NewGuid();
            var groupRepository = GroupRepository(groupKey, owner, owner, payer, participant);
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.CreateWithSharesAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expense expense, CancellationToken _) => (expense, true));
            var useCase = new CreateExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(Request(owner, groupKey, payer.UniqueKey, [participant.UniqueKey]), CancellationToken.None);

            Assert.True(response.Succeeded);
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.Is<Expense>(expense =>
                    expense.Payer!.UniqueKey == payer.UniqueKey &&
                    expense.Shares.Select(share => share.Participant.UniqueKey).SequenceEqual(new[] { participant.UniqueKey })),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("EUR", "10", "Only USD expenses are supported.")]
        [InlineData("USD", "10.001", "Expense amounts may have at most two decimal places.")]
        [InlineData("USD", "0", "Expense amount must be greater than zero.")]
        [InlineData("USD", "10000000000000000", "Expense amount exceeds the supported range.")]
        public async Task CreateExpense_WithUnsupportedMoneyValue_ReturnsValidationError(
            string currency,
            string amount,
            string expectedMessage)
        {
            var expenseRepository = new Mock<IExpenseRepository>();
            var useCase = new CreateExpense(expenseRepository.Object, Mock.Of<IGroupRepository>());

            var response = await useCase.Handle(new CreateExpenseRequest
            {
                ActorKey = Guid.NewGuid(),
                GroupKey = Guid.NewGuid(),
                IdempotencyKey = Guid.NewGuid(),
                Description = "Expense",
                Currency = currency,
                Amount = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture),
                PayerKey = Guid.NewGuid(),
                ParticipantKeys = [Guid.NewGuid()]
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains(expectedMessage, response.ValidationMessages!);
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [MemberData(nameof(InvalidParticipantLists))]
        public async Task CreateExpense_WithInvalidParticipants_ReturnsValidationErrorWithoutCreatingExpense(
            List<Guid>? participantKeys,
            string expectedMessage)
        {
            var expenseRepository = new Mock<IExpenseRepository>();
            var useCase = new CreateExpense(expenseRepository.Object, Mock.Of<IGroupRepository>());

            var response = await useCase.Handle(new CreateExpenseRequest
            {
                ActorKey = Guid.NewGuid(),
                GroupKey = Guid.NewGuid(),
                IdempotencyKey = Guid.NewGuid(),
                Description = "Expense",
                Currency = "USD",
                Amount = 10m,
                PayerKey = Guid.NewGuid(),
                ParticipantKeys = participantKeys!
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains(expectedMessage, response.ValidationMessages!);
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        public static IEnumerable<object[]> InvalidParticipantLists()
        {
            yield return [null!, "Select at least one expense participant."];
            yield return [new List<Guid>(), "Select at least one expense participant."];
            var duplicateKey = Guid.NewGuid();
            yield return [new List<Guid> { duplicateKey, duplicateKey }, "Expense participants must be unique."];
            yield return [new List<Guid> { Guid.Empty }, "Expense participant keys must be valid."];
        }

        [Fact]
        public async Task CreateExpense_WhenParticipantIsNotAnActiveMember_ReturnsValidationFailure()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var groupKey = Guid.NewGuid();
            var groupRepository = GroupRepository(groupKey, owner, owner);
            var expenseRepository = new Mock<IExpenseRepository>();
            var useCase = new CreateExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(Request(owner, groupKey, owner.UniqueKey, [Guid.NewGuid()]), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Every participant must be an active member of the group.", response.ValidationMessages!);
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateExpense_WhenKeyReusedWithSameDetails_ReturnsReplayedExpense()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var groupKey = Guid.NewGuid();
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.CreateWithSharesAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expense expense, CancellationToken _) => (StoredCopy(expense, expense.Amount), false));
            var useCase = new CreateExpense(
                expenseRepository.Object,
                GroupRepository(groupKey, owner, owner).Object);

            var response = await useCase.Handle(Request(owner, groupKey, owner.UniqueKey, [owner.UniqueKey]), CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(77, response.Result!.Id);
            Assert.True(response.Result.Replayed);
        }

        [Fact]
        public async Task CreateExpense_WhenReplayedParticipantIsNoLongerActive_ReturnsReplayedExpense()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var removedParticipant = Member(Guid.NewGuid(), 2);
            var groupKey = Guid.NewGuid();
            var request = Request(owner, groupKey, owner.UniqueKey, [removedParticipant.UniqueKey]);
            var stored = new Expense
            {
                Id = 77,
                UniqueKey = request.IdempotencyKey,
                Description = request.Description,
                Currency = "USD",
                Amount = request.Amount,
                BelongsTo = new Group { Id = 12, UniqueKey = groupKey },
                CreatedBy = owner,
                Payer = owner,
                Shares = [new ExpenseShare { Amount = request.Amount, Participant = removedParticipant }]
            };
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.FindByUniqueKeyAsync(request.IdempotencyKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stored);
            var useCase = new CreateExpense(
                expenseRepository.Object,
                GroupRepository(groupKey, owner, owner).Object);

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(stored.Id, response.Result!.Id);
            Assert.True(response.Result.Replayed);
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateExpense_WhenKeyReusedWithDifferentDetails_ReturnsInvalid()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var groupKey = Guid.NewGuid();
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.CreateWithSharesAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expense expense, CancellationToken _) => (StoredCopy(expense, expense.Amount + 1m), false));
            var useCase = new CreateExpense(
                expenseRepository.Object,
                GroupRepository(groupKey, owner, owner).Object);

            var response = await useCase.Handle(Request(owner, groupKey, owner.UniqueKey, [owner.UniqueKey]), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("request key was already used", response.ValidationMessages!.Single());
        }

        [Fact]
        public async Task CreateExpense_WhenMembershipConflictsAtWriteTime_ReturnsValidationError()
        {
            var owner = Member(Guid.NewGuid(), 1);
            var groupKey = Guid.NewGuid();
            const string message = "The payer and every participant must still be members of the group.";
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository
                .Setup(repository => repository.CreateWithSharesAsync(It.IsAny<Expense>(), CancellationToken.None))
                .ThrowsAsync(new GroupMembershipConflictException(message));
            var useCase = new CreateExpense(
                expenseRepository.Object,
                GroupRepository(groupKey, owner, owner).Object);

            var response = await useCase.Handle(
                Request(owner, groupKey, owner.UniqueKey, [owner.UniqueKey]), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Equal(message, Assert.Single(response.ValidationMessages!));
            expenseRepository.Verify(repository => repository.CreateWithSharesAsync(
                It.IsAny<Expense>(), CancellationToken.None), Times.Once);
        }

        private static Expense StoredCopy(Expense requested, decimal amount) => new()
        {
            Id = 77,
            UniqueKey = requested.UniqueKey,
            Description = requested.Description,
            Currency = requested.Currency,
            Amount = amount,
            BelongsTo = requested.BelongsTo,
            CreatedBy = requested.CreatedBy,
            Payer = requested.Payer,
            Shares = requested.Shares
                .Select(share => new ExpenseShare { Participant = share.Participant, Amount = share.Amount })
                .ToList()
        };

        private static CreateExpenseRequest Request(User actor, Guid groupKey, Guid payerKey, List<Guid> participantKeys) => new()
        {
            ActorKey = actor.UniqueKey,
            GroupKey = groupKey,
            IdempotencyKey = Guid.NewGuid(),
            Description = "Expense",
            Currency = "USD",
            Amount = 15.25m,
            PayerKey = payerKey,
            ParticipantKeys = participantKeys
        };

        private static Mock<IGroupRepository> GroupRepository(Guid groupKey, User owner, params User[] members)
        {
            var repository = new Mock<IGroupRepository>();
            repository.Setup(groupRepository => groupRepository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group { Id = 12, UniqueKey = groupKey, Owner = owner, Members = members.ToList() });
            return repository;
        }

        private static User Member(Guid key, int id) => new() { UniqueKey = key, Id = id, Name = $"Member {id}" };
    }
}
