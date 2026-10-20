using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class VoidExpenseTests
    {
        [Fact]
        public async Task VoidExpense_WhenOrganizerVoidsExpense_MarksItVoided()
        {
            var ownerKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var expenseKey = Guid.NewGuid();
            var group = new Group { Id = 5, UniqueKey = groupKey, Owner = new User { UniqueKey = ownerKey } };
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey)).ReturnsAsync(group);
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository.Setup(repository => repository.VoidAsync(expenseKey, group.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VoidExpenseOutcome(new Expense { UniqueKey = expenseKey, Voided = true }, false));
            var useCase = new VoidExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(new VoidExpenseRequest
            {
                ActorKey = ownerKey,
                GroupKey = groupKey,
                ExpenseKey = expenseKey
            }, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.True(response.Result!.Voided);
        }

        [Fact]
        public async Task VoidExpense_WhenGroupHasRepayments_RejectsVoid()
        {
            var ownerKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var expenseKey = Guid.NewGuid();
            var group = new Group { Id = 5, UniqueKey = groupKey, Owner = new User { UniqueKey = ownerKey } };
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey)).ReturnsAsync(group);
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository.Setup(repository => repository.VoidAsync(expenseKey, group.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VoidExpenseOutcome(null, true));
            var useCase = new VoidExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(new VoidExpenseRequest
            {
                ActorKey = ownerKey,
                GroupKey = groupKey,
                ExpenseKey = expenseKey
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("after repayments have been recorded", response.ValidationMessages!.Single());
        }

        [Fact]
        public async Task VoidExpense_WhenActorIsNotOrganizer_DoesNotChangeExpense()
        {
            var groupKey = Guid.NewGuid();
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group { Owner = new User { UniqueKey = Guid.NewGuid() } });
            var expenseRepository = new Mock<IExpenseRepository>();
            var useCase = new VoidExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(new VoidExpenseRequest
            {
                ActorKey = Guid.NewGuid(),
                GroupKey = groupKey,
                ExpenseKey = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Only the group organizer can void expenses.", response.ValidationMessages!);
            expenseRepository.Verify(repository => repository.VoidAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
