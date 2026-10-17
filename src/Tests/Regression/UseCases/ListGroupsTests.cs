using Application.UseCases;
using Application.UseCases.Ports;
using Moq;

namespace Tests.Regression.UseCases;

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
            Mock.Of<IExpenseRepository>());

        var response = await useCase.Handle(new ListGroupsRequest { ActorKey = actorKey }, CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Empty(response.Result!.Groups);
    }
}
