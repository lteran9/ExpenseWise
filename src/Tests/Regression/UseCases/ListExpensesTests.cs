using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases;

public class ListExpensesTests
{
    [Fact]
    public async Task ListExpenses_WhenGroupHasNoExpenses_ReturnsEmptySuccess()
    {
        var groupKey = Guid.NewGuid();
        var actorKey = Guid.NewGuid();
        var groupRepository = new Mock<IGroupRepository>();
        groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
            .ReturnsAsync(new Group
            {
                UniqueKey = groupKey,
                Members = [new User { UniqueKey = actorKey }]
            });
        var expenseRepository = new Mock<IExpenseRepository>();
        expenseRepository.Setup(repository => repository.GetGroupExpenses(1))
            .ReturnsAsync((List<Expense>?)null);

        var useCase = new ListExpenses(groupRepository.Object, expenseRepository.Object);
        var response = await useCase.Handle(new ListExpensesRequest
        {
            GroupKey = groupKey,
            ActorKey = actorKey
        }, CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Empty(response.Result!.Expenses);
    }

    [Fact]
    public async Task ListExpenses_WhenActorKeyIsEmpty_ReturnsValidationFailure()
    {
        var groupRepository = new Mock<IGroupRepository>();
        var useCase = new ListExpenses(groupRepository.Object, Mock.Of<IExpenseRepository>());

        var response = await useCase.Handle(new ListExpensesRequest
        {
            GroupKey = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.False(response.Succeeded);
        Assert.Contains("An authenticated user is required.", response.ValidationMessages!);
        groupRepository.Verify(repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
    }
}
