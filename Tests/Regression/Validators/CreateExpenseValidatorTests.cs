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
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_DescriptionNotEmpty(
           [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
           CreateExpense useCase)
        {
            // Arrange
            var createExpense =
               new CreateExpenseRequest()
               {
                   Currency = "USD",
                   Amount = 10.00M
               };

            // Act
            var response = await useCase.Handle(createExpense, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_CurrencyNotEmpty(
           [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
           CreateExpense useCase)
        {
            // Arrange
            var createExpense =
               new CreateExpenseRequest()
               {
                   Description = "This is a test expense.",
                   Amount = 10.00M
               };

            // Act
            var response = await useCase.Handle(createExpense, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_AmountNotZero(
           [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
           CreateExpense useCase)
        {
            // Arrange
            var createExpense =
               new CreateExpenseRequest()
               {
                   Description = "This is a test expense.",
                   Currency = "USD"
               };

            // Act
            var response = await useCase.Handle(createExpense, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
