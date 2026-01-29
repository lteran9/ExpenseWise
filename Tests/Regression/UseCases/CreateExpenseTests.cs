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
            var request = new CreateExpenseRequest
            {
                Description = "",
                Currency = "USD",
                Amount = 100.00M
            };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithEmptyCurrency_ShouldFail(
            CreateExpense useCase)
        {
            var request = new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "",
                Amount = 100.00M
            };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithZeroAmount_ShouldFail(
            CreateExpense useCase)
        {
            var request = new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "USD",
                Amount = 0
            };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithNegativeAmount_ShouldFail(
            CreateExpense useCase)
        {
            var request = new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "USD",
                Amount = -50.00M
            };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
        }

        #endregion

        #region Repository Error Handling

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WhenRepositoryReturnsNull_ShouldFail(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            // Arrange
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync((Expense?)null);

            var request = new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "USD",
                Amount = 100.00M
            };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WhenRepositoryThrows_ShouldFail(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var request = new CreateExpenseRequest
            {
                Description = "Test expense",
                Currency = "USD",
                Amount = 100.00M
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.Handle(request, CancellationToken.None));
        }

        #endregion

        #region Verify Correct Data Mapping

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithValidRequest_MapsDataCorrectly(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            // Arrange
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 42 });

            var request = new CreateExpenseRequest
            {
                Description = "Coffee",
                Currency = "EUR",
                Amount = 5.50M
            };

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
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            var request = new CreateExpenseRequest
            {
                Description = "Large expense",
                Currency = "USD",
                Amount = 999999.99M
            };

            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithSmallAmount_ShouldSucceed(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            var request = new CreateExpenseRequest
            {
                Description = "Small expense",
                Currency = "USD",
                Amount = 0.01M
            };

            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task CreateExpense_WithVeryLongDescription_ShouldSucceed(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            var longDescription = new string('A', 1000);
            var request = new CreateExpenseRequest
            {
                Description = longDescription,
                Currency = "USD",
                Amount = 100.00M
            };

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
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            // Arrange
            var expectedId = 12345;
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = expectedId });

            var request = new CreateExpenseRequest
            {
                Description = "Test",
                Currency = "USD",
                Amount = 100.00M
            };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(response.Succeeded);
            Assert.NotNull(response.Result);
            Assert.Equal(expectedId, response.Result.Id);
        }

        #endregion
    }
}
