using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class RetrieveExpenseTests
    {
        [Fact]
        public async Task RetrieveExpense_MemberRequestsExpense_ReturnsExpense()
        {
            var actorKey = Guid.NewGuid();
            var expenseKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var expense = new Expense
            {
                UniqueKey = expenseKey,
                BelongsTo = new Group { UniqueKey = groupKey }
            };
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository.Setup(repository => repository.FindByUniqueKeyAsync(expenseKey, CancellationToken.None))
                .ReturnsAsync(expense);
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Members = [new User { UniqueKey = actorKey }]
                });

            var useCase = new RetrieveExpense(expenseRepository.Object, groupRepository.Object);
            var response = await useCase.Handle(
                new RetrieveExpenseRequest { ActorKey = actorKey, ExpenseKey = expenseKey },
                CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Same(expense, response.Result!.Expense);
        }

        [Fact]
        public async Task RetrieveExpense_NonmemberRequestsExpense_ReturnsNotFoundWithoutExpense()
        {
            var actorKey = Guid.NewGuid();
            var expenseKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository.Setup(repository => repository.FindByUniqueKeyAsync(expenseKey, CancellationToken.None))
                .ReturnsAsync(new Expense
                {
                    UniqueKey = expenseKey,
                    BelongsTo = new Group { UniqueKey = groupKey }
                });
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Members = [new User { UniqueKey = Guid.NewGuid() }]
                });

            var useCase = new RetrieveExpense(expenseRepository.Object, groupRepository.Object);
            var response = await useCase.Handle(
                new RetrieveExpenseRequest { ActorKey = actorKey, ExpenseKey = expenseKey },
                CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Expense not found.", response.ValidationMessages!);
            Assert.Null(response.Result);
        }

        [Fact]
        public async Task RetrieveExpense_WithEmptyActorKey_ReturnsValidationFailure()
        {
            var expenseRepository = new Mock<IExpenseRepository>();
            var groupRepository = new Mock<IGroupRepository>();
            var useCase = new RetrieveExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(
                new RetrieveExpenseRequest { ExpenseKey = Guid.NewGuid() },
                CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("An authenticated user and expense key are required.", response.ValidationMessages!);
            expenseRepository.Verify(
                repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
            groupRepository.Verify(repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RetrieveExpense_WithEmptyExpenseKey_ReturnsValidationFailure()
        {
            var expenseRepository = new Mock<IExpenseRepository>();
            var groupRepository = new Mock<IGroupRepository>();
            var useCase = new RetrieveExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(
                new RetrieveExpenseRequest { ActorKey = Guid.NewGuid() },
                CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("An authenticated user and expense key are required.", response.ValidationMessages!);
            expenseRepository.Verify(
                repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
            groupRepository.Verify(repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RetrieveExpense_WhenExpenseDoesNotExist_ReturnsNotFound()
        {
            var expenseKey = Guid.NewGuid();
            var expenseRepository = new Mock<IExpenseRepository>();
            expenseRepository.Setup(repository => repository.FindByUniqueKeyAsync(expenseKey, CancellationToken.None))
                .ReturnsAsync((Expense?)null);
            var groupRepository = new Mock<IGroupRepository>();
            var useCase = new RetrieveExpense(expenseRepository.Object, groupRepository.Object);

            var response = await useCase.Handle(
                new RetrieveExpenseRequest { ActorKey = Guid.NewGuid(), ExpenseKey = expenseKey },
                CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Expense not found.", response.ValidationMessages!);
            Assert.Null(response.Result);
            groupRepository.Verify(repository => repository.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
