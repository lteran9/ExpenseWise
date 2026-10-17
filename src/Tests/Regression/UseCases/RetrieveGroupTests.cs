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
        var useCase = new RetrieveGroup(repository.Object);

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

        var useCase = new RetrieveGroup(repository.Object);
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

        var useCase = new RetrieveGroup(repository.Object);
        var result = await useCase.Handle(
            new RetrieveGroupRequest { UniqueKey = groupKey, ActorKey = actorKey },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Result!.Members);
    }
}
