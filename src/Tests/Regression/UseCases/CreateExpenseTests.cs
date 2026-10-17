using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class CreateExpenseTests
    {
        #region Invalid Input Validation Tests

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithEmptyDescription_ShouldFail(
            CreateExpense useCase)
        {
            // Arrange
            var request =
                new CreateExpenseRequest
                {
                    Description = "",
                    Currency = "USD",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithEmptyActorKey_ShouldFail(
            CreateExpense useCase)
        {
            var response = await useCase.Handle(new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "USD",
                Amount = 10.00M,
                GroupKey = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("An authenticated user is required.", response.ValidationMessages!);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithEmptyCurrency_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Test expense",
                    Currency = "",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithZeroAmount_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Test expense",
                    Currency = "USD",
                    Amount = 0,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithNegativeAmount_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Test expense",
                    Currency = "USD",
                    Amount = -50.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        #endregion

        #region Repository Error Handling

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WhenRepositoryReturnsNull_ShouldFail(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            // Arrange
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync((Expense?)null);

            var request =
                new CreateExpenseRequest
                {
                    Description = "Test expense",
                    Currency = "USD",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WhenRepositoryThrows_ShouldFail(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var request =
                new CreateExpenseRequest
                {
                    Description = "Test expense",
                    Currency = "USD",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.Handle(request, CancellationToken.None));
        }

        #endregion

        #region Verify Correct Data Mapping

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithValidRequest_MapsDataCorrectly(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            // Arrange
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 42 });

            var request =
                new CreateExpenseRequest
                {
                    Description = "Coffee",
                    Currency = "EUR",
                    Amount = 5.50M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            // Act
            await useCase.Handle(request, CancellationToken.None);

            // Assert
            mockRepository.Verify(
                r => r.CreateAsync(It.Is<Expense>(e =>
                    e.Description == "Coffee" &&
                    e.Currency == "EUR" &&
                    e.Amount == 5.50M)),
                Times.Once);
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithLargeAmount_ShouldSucceed(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Large expense",
                    Currency = "USD",
                    Amount = 999999.99M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithSmallAmount_ShouldSucceed(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Small expense",
                    Currency = "USD",
                    Amount = 0.01M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithVeryLongDescription_ShouldSucceed(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            var longDescription = new string('A', 1000);
            var request =
                new CreateExpenseRequest
                {
                    Description = longDescription,
                    Currency = "USD",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        #endregion

        #region Response Validation

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithValidRequest_ReturnsCorrectId(
            [Frozen] Mock<IExpenseRepository> mockRepository,
            [Frozen] Mock<IGroupRepository> groupRepository,
            [Frozen] Mock<IUserRepository> userRepository,
            CreateExpense useCase)
        {
            // Arrange
            var expectedId = 12345;
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = expectedId });

            var request =
                new CreateExpenseRequest
                {
                    Description = "Test",
                    Currency = "USD",
                    Amount = 100.00M,
                    ActorKey = Guid.NewGuid(),
                    GroupKey = Guid.NewGuid()
                };
            AuthorizeOrganizer(groupRepository, userRepository, request);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(response.Succeeded);
            Assert.NotNull(response.Result);
            Assert.Equal(expectedId, response.Result.Id);
        }

        #endregion

        [Fact]
        public async Task CreateExpense_WhenActorIsNotOrganizer_DoesNotCreateExpense()
        {
            var actorKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            var expenseRepository = new Mock<IExpenseRepository>();
            var userRepository = new Mock<IUserRepository>();
            var groupRepository = new Mock<IGroupRepository>();
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = Guid.NewGuid() }
                });
            userRepository.Setup(repository => repository.FindByUniqueKeyAsync(actorKey))
                .ReturnsAsync(new User { UniqueKey = actorKey });
            var useCase = new CreateExpense(
                expenseRepository.Object,
                userRepository.Object,
                groupRepository.Object);

            var response = await useCase.Handle(new CreateExpenseRequest
            {
                Description = "Unauthorized expense",
                Currency = "USD",
                Amount = 10,
                ActorKey = actorKey,
                GroupKey = groupKey
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            expenseRepository.Verify(repository => repository.CreateAsync(It.IsAny<Expense>()), Times.Never);
        }

        private static void AuthorizeOrganizer(
            Mock<IGroupRepository> groupRepository,
            Mock<IUserRepository> userRepository,
            CreateExpenseRequest request)
        {
            groupRepository.Setup(repository => repository.FindByUniqueKeyAsync(request.GroupKey))
                .ReturnsAsync(new Group
                {
                    Id = 1,
                    UniqueKey = request.GroupKey,
                    Owner = new User { UniqueKey = request.ActorKey }
                });
            userRepository.Setup(repository => repository.FindByUniqueKeyAsync(request.ActorKey))
                .ReturnsAsync(new User { UniqueKey = request.ActorKey });
        }
    }
}
