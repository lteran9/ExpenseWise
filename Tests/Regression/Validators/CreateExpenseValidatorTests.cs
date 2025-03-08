using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateExpenseValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_DescriptionNotEmpty([Frozen] Mock<ISqlDatabase<Expense>> mockRepository)
      {
         var createExpense =
            new CreateExpenseRequest()
            {
               Currency = "USD",
               Amount = 10.00M
            };
         var handler = new CreateExpense(mockRepository.Object);
         var response = await handler.Handle(createExpense, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_CurrencyNotEmpty([Frozen] Mock<ISqlDatabase<Expense>> mockRepository)
      {
         var createExpense =
            new CreateExpenseRequest()
            {
               Description = "This is a test expense.",
               Amount = 10.00M
            };
         var handler = new CreateExpense(mockRepository.Object);
         var response = await handler.Handle(createExpense, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_AmountNotZero([Frozen] Mock<ISqlDatabase<Expense>> mockRepository)
      {
         var createExpense =
            new CreateExpenseRequest()
            {
               Description = "This is a test expense.",
               Currency = "USD"
            };
         var handler = new CreateExpense(mockRepository.Object);
         var response = await handler.Handle(createExpense, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}