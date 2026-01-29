using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class CreateExpenseValidatorTests
    {
        #region Valid Input Should Pass

        [Theory]
        [AutoMoq]
        public async Task Validate_WithValidInput_ShouldSucceed(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            // Arrange
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var request =
                new CreateExpenseRequest
                {
                    Description = "Valid expense",
                    Currency = "USD",
                    Amount = 100.00M
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(response.Succeeded);
        }

        #endregion

        #region Null Values (Not Just Empty Strings)

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        [Theory]
        [AutoMoq]
        public async Task Validate_WithNullDescription_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = null,
                    Currency = "USD",
                    Amount = 10.00M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_WithNullCurrency_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Valid description",
                    Currency = null,
                    Amount = 10.00M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        #endregion

        #region Whitespace-Only Strings

        [Theory]
        [AutoMoq]
        public async Task Validate_WithWhitespaceOnlyDescription_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "   ",
                    Currency = "USD",
                    Amount = 10.00M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_WithWhitespaceOnlyCurrency_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Valid description",
                    Currency = "   ",
                    Amount = 10.00M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        #endregion

        #region Multiple Validation Errors

        [Theory]
        [AutoMoq]
        public async Task Validate_WithMultipleErrors_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "",
                    Currency = "",
                    Amount = 0
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        #endregion

        #region Amount Boundary Cases

        [Theory]
        [AutoMoq]
        public async Task Validate_WithNegativeAmount_ShouldFail(
            CreateExpense useCase)
        {
            var request =
                new CreateExpenseRequest
                {
                    Description = "Valid",
                    Currency = "USD",
                    Amount = -50.00M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_WithMinimalValidAmount_ShouldSucceed(
            [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
            CreateExpense useCase)
        {
            mockRepository.Setup(r => r.CreateAsync(It.IsAny<Expense>()))
                .ReturnsAsync(new Expense { Id = 1 });

            var request =
                new CreateExpenseRequest
                {
                    Description = "Valid",
                    Currency = "USD",
                    Amount = 0.01M
                };

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        #endregion
    }
}
