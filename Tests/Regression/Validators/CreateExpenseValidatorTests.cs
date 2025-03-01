using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateExpenseValidatorTests
   {
      [Fact]
      public async Task Validate_DescriptionNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<Expense>>();
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

      [Fact]
      public async Task Validate_CurrencyNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<Expense>>();
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

      [Fact]
      public async Task Validate_AmountNotZero()
      {
         var mockRepository = new Mock<ISqlDatabase<Expense>>();
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