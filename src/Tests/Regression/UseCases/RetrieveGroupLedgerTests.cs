using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class RetrieveGroupLedgerTests
    {
        private readonly Guid _groupKey = Guid.NewGuid();
        private readonly User _ana = new() { Id = 1, UniqueKey = Guid.NewGuid(), Name = "Ana" };
        private readonly User _ben = new() { Id = 2, UniqueKey = Guid.NewGuid(), Name = "Ben" };
        private readonly User _cy = new() { Id = 3, UniqueKey = Guid.NewGuid(), Name = "Cy" };
        private readonly Group _group;

        public RetrieveGroupLedgerTests()
        {
            _group = new Group
            {
                Id = 9,
                UniqueKey = _groupKey,
                Owner = _cy,
                Members = [_ana, _ben, _cy]
            };
        }

        private Expense DinnerPaidByCy() => new()
        {
            Amount = 40m,
            Payer = _cy,
            Shares =
            [
                new ExpenseShare { Participant = _ana, Amount = 20m },
                new ExpenseShare { Participant = _ben, Amount = 20m }
            ]
        };

        private static RetrieveGroupLedgerRequest Request(Guid actorKey, Guid groupKey) => new()
        {
            ActorKey = actorKey,
            GroupKey = groupKey
        };

        private static Mock<IGroupRepository> GroupRepositoryReturning(Group group)
        {
            var repository = new Mock<IGroupRepository>();
            repository.Setup(value => value.FindByUniqueKeyAsync(group.UniqueKey)).ReturnsAsync(group);
            return repository;
        }

        private static Mock<IExpenseRepository> ExpenseRepositoryReturning(List<Expense> expenses)
        {
            var repository = new Mock<IExpenseRepository>();
            repository.Setup(value => value.GetGroupExpenses(It.IsAny<int>())).ReturnsAsync(expenses);
            return repository;
        }

        private static Mock<IRepaymentRepository> RepaymentRepositoryReturning(List<Repayment> repayments)
        {
            var repository = new Mock<IRepaymentRepository>();
            repository.Setup(value => value.GetGroupRepaymentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(repayments);
            return repository;
        }

        [Fact]
        public async Task RetrieveGroupLedger_WhenMemberRequestsLedger_ReturnsTotalsBalancesAndSuggestions()
        {
            var useCase = new RetrieveGroupLedger(
                GroupRepositoryReturning(_group).Object,
                ExpenseRepositoryReturning([DinnerPaidByCy()]).Object,
                RepaymentRepositoryReturning([]).Object);

            var response = await useCase.Handle(Request(_ana.UniqueKey, _groupKey), CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(40m, response.Result!.TotalSpent);
            Assert.Equal(40m, response.Result.Outstanding);
            Assert.Equal(-20m, response.Result.Balances.Single(balance => balance.UniqueKey == _ana.UniqueKey).NetBalance);
            Assert.Equal(40m, response.Result.Balances.Single(balance => balance.UniqueKey == _cy.UniqueKey).NetBalance);
            Assert.Equal(2, response.Result.Suggestions.Count);
            Assert.All(response.Result.Suggestions, suggestion => Assert.Equal(_cy.UniqueKey, suggestion.ToKey));
            Assert.Equal(40m, response.Result.Suggestions.Sum(suggestion => suggestion.Amount));
        }

        [Fact]
        public async Task RetrieveGroupLedger_WhenRepaymentsExist_ReturnsHistoryNewestFirst()
        {
            var older = new Repayment
            {
                UniqueKey = Guid.NewGuid(),
                Amount = 5m,
                Currency = "USD",
                OccurredAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                Sender = _ana,
                Recipient = _cy
            };
            var newer = new Repayment
            {
                UniqueKey = Guid.NewGuid(),
                Amount = 15m,
                Currency = "USD",
                OccurredAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc),
                Sender = _ben,
                Recipient = _cy
            };
            var useCase = new RetrieveGroupLedger(
                GroupRepositoryReturning(_group).Object,
                ExpenseRepositoryReturning([DinnerPaidByCy()]).Object,
                RepaymentRepositoryReturning([older, newer]).Object);

            var response = await useCase.Handle(Request(_cy.UniqueKey, _groupKey), CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(2, response.Result!.Repayments.Count);
            Assert.Equal(newer.UniqueKey, response.Result.Repayments[0].RepaymentKey);
            Assert.Equal("Ben", response.Result.Repayments[0].SenderName);
            Assert.Equal("Cy", response.Result.Repayments[0].RecipientName);
            Assert.Equal(15m, response.Result.Repayments[0].Amount);
        }

        [Fact]
        public async Task RetrieveGroupLedger_WhenActorIsNotMember_ReturnsInvalidWithoutLoadingLedger()
        {
            var outsider = new User { Id = 4, UniqueKey = Guid.NewGuid(), Name = "Outsider" };
            var expenseRepository = ExpenseRepositoryReturning([DinnerPaidByCy()]);
            var useCase = new RetrieveGroupLedger(
                GroupRepositoryReturning(_group).Object,
                expenseRepository.Object,
                RepaymentRepositoryReturning([]).Object);

            var response = await useCase.Handle(Request(outsider.UniqueKey, _groupKey), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Only group members can view the ledger.", response.ValidationMessages!);
            expenseRepository.Verify(value => value.GetGroupExpenses(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RetrieveGroupLedger_WhenGroupDoesNotExist_ReturnsInvalid()
        {
            var unknownGroupKey = Guid.NewGuid();
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(value => value.FindByUniqueKeyAsync(unknownGroupKey)).ReturnsAsync((Group?)null);
            var useCase = new RetrieveGroupLedger(
                groupRepository.Object,
                ExpenseRepositoryReturning([]).Object,
                RepaymentRepositoryReturning([]).Object);

            var response = await useCase.Handle(Request(_ana.UniqueKey, unknownGroupKey), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Unable to find a group for the given key.", response.ValidationMessages!);
        }
    }
}
