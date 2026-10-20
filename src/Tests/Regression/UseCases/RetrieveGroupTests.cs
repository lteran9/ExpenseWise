using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases;

public class RetrieveGroupTests
{
    [Fact]
    public async Task RetrieveGroup_WithEmptyActorKey_ReturnsValidationFailure()
    {
        var repository = new Mock<IGroupRepository>();
        var useCase = new RetrieveGroup(repository.Object, Mock.Of<IRepaymentRepository>());

        var result = await useCase.Handle(
            new RetrieveGroupRequest { UniqueKey = Guid.NewGuid() },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("An authenticated user is required.", result.ValidationMessages!);
        repository.Verify(value => value.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RetrieveGroup_NonmemberRequestsGroup_ReturnsNotFound()
    {
        var groupKey = Guid.NewGuid();
        var actorKey = Guid.NewGuid();
        var repository = new Mock<IGroupRepository>();
        repository.Setup(value => value.FindByUniqueKeyAsync(groupKey))
            .ReturnsAsync(new Group
            {
                UniqueKey = groupKey,
                Owner = new User { UniqueKey = Guid.NewGuid() },
                Members = [new User { UniqueKey = Guid.NewGuid() }]
            });

        var useCase = new RetrieveGroup(repository.Object, Mock.Of<IRepaymentRepository>());
        var result = await useCase.Handle(
            new RetrieveGroupRequest { UniqueKey = groupKey, ActorKey = actorKey },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Group not found", result.ValidationMessages!.Single());
    }

    [Fact]
    public async Task RetrieveGroup_MemberRequestsGroup_ReturnsGroupAndMembers()
    {
        var groupKey = Guid.NewGuid();
        var actorKey = Guid.NewGuid();
        var repository = new Mock<IGroupRepository>();
        repository.Setup(value => value.FindByUniqueKeyAsync(groupKey))
            .ReturnsAsync(new Group
            {
                UniqueKey = groupKey,
                Owner = new User { UniqueKey = actorKey },
                Members = [new User { UniqueKey = actorKey }]
            });

        var useCase = new RetrieveGroup(repository.Object, EmptyLedgerRepository().Object);
        var result = await useCase.Handle(
            new RetrieveGroupRequest { UniqueKey = groupKey, ActorKey = actorKey },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Result!.Members);
    }

    [Fact]
    public async Task RetrieveGroup_WithExpensesAndRepayments_ReturnsLedgerTotals()
    {
        var organizer = new User { UniqueKey = Guid.NewGuid() };
        var member = new User { UniqueKey = Guid.NewGuid() };
        var group = new Group
        {
            Id = 7,
            UniqueKey = Guid.NewGuid(),
            Owner = organizer,
            Members = [organizer, member]
        };
        var repository = new Mock<IGroupRepository>();
        repository.Setup(value => value.FindByUniqueKeyAsync(group.UniqueKey)).ReturnsAsync(group);
        var repaymentRepository = new Mock<IRepaymentRepository>();
        repaymentRepository.Setup(value => value.GetGroupLedgerSnapshotAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupLedgerSnapshot(new GroupLedger(group.Members,
                [
                    new Expense
                    {
                        Amount = 20m,
                        Payer = organizer,
                        Shares =
                        [
                            new ExpenseShare { Participant = organizer, Amount = 10m },
                            new ExpenseShare { Participant = member, Amount = 10m }
                        ]
                    }
                ],
                [new Repayment { Sender = member, Recipient = organizer, Amount = 4m }]), []));

        var useCase = new RetrieveGroup(repository.Object, repaymentRepository.Object);
        var result = await useCase.Handle(
            new RetrieveGroupRequest { UniqueKey = group.UniqueKey, ActorKey = member.UniqueKey },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(20m, result.Result!.Expensed);
        Assert.Equal(6m, result.Result.Outstanding);
    }

    private static Mock<IRepaymentRepository> EmptyLedgerRepository()
    {
        var repaymentRepository = new Mock<IRepaymentRepository>();
        repaymentRepository.Setup(value => value.GetGroupLedgerSnapshotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupLedgerSnapshot(new GroupLedger([], [], []), []));
        return repaymentRepository;
    }
}
