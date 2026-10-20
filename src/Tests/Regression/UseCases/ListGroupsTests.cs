using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{

    public class ListGroupsTests
    {
        [Fact]
        public async Task ListGroups_WhenUserHasNoGroups_ReturnsEmptySuccess()
        {
            var actorKey = Guid.NewGuid();
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.ListAsync(actorKey))
                .ReturnsAsync([]);
            var useCase = new ListGroups(
                Mock.Of<IUserRepository>(),
                groupRepository.Object,
                Mock.Of<IRepaymentRepository>());

            var response = await useCase.Handle(new ListGroupsRequest { ActorKey = actorKey }, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Empty(response.Result!.Groups);
        }

        [Fact]
        public async Task ListGroups_CalculatesTotalsFromNonvoidPayersAndShares()
        {
            var actor = new User { UniqueKey = Guid.NewGuid() };
            var participant = new User { UniqueKey = Guid.NewGuid() };
            var group = new Group
            {
                Id = 5,
                UniqueKey = Guid.NewGuid(),
                Owner = actor,
                Members = [actor, participant]
            };
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.ListAsync(actor.UniqueKey)).ReturnsAsync([group]);
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(repository => repository.GetGroupLedgerSnapshotAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(
                new GroupLedgerSnapshot(new GroupLedger(group.Members,
            [
                new Expense
            {
                Amount = 10m,
                Payer = actor,
                Shares = [new ExpenseShare { Participant = participant, Amount = 10m }]
            },
            new Expense
            {
                Amount = 90m,
                Voided = true,
                Payer = actor,
                Shares = [new ExpenseShare { Participant = actor, Amount = 90m }]
            }
            ], []), []));
            var useCase = new ListGroups(Mock.Of<IUserRepository>(), groupRepository.Object, repaymentRepository.Object);

            var response = await useCase.Handle(new ListGroupsRequest { ActorKey = actor.UniqueKey }, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(10m, response.Result!.Groups.Single().Expensed);
            Assert.Equal(10m, response.Result.Groups.Single().Outstanding);
        }

        [Fact]
        public async Task ListGroups_WhenRepaymentRecorded_ReducesOutstanding()
        {
            var actor = new User { UniqueKey = Guid.NewGuid() };
            var participant = new User { UniqueKey = Guid.NewGuid() };
            var group = new Group
            {
                Id = 5,
                UniqueKey = Guid.NewGuid(),
                Owner = actor,
                Members = [actor, participant]
            };
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.ListAsync(actor.UniqueKey)).ReturnsAsync([group]);
            var expenses = new List<Expense>
            {
                new Expense
                {
                    Amount = 10m,
                    Payer = actor,
                    Shares = [new ExpenseShare { Participant = participant, Amount = 10m }]
                }
            };
            var repayments = new List<Repayment>
            {
                new Repayment
                {
                    Amount = 4m,
                    Currency = "USD",
                    Sender = participant,
                    Recipient = actor
                }
            };
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(repository => repository.GetGroupLedgerSnapshotAsync(group.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GroupLedgerSnapshot(new GroupLedger(group.Members, expenses, repayments), repayments));
            var useCase = new ListGroups(Mock.Of<IUserRepository>(), groupRepository.Object, repaymentRepository.Object);

            var response = await useCase.Handle(new ListGroupsRequest { ActorKey = actor.UniqueKey }, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(10m, response.Result!.Groups.Single().Expensed);
            Assert.Equal(6m, response.Result.Groups.Single().Outstanding);
        }
    }
}
