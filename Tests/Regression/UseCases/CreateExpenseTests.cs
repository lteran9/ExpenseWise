using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateExpenseTests
   {
      [Theory]
      [AutoMoq]
      public async Task CreateExpenseMoq([Frozen] Mock<ISqlDatabase<Expense>> mockRepository)
      {
         var mockExpense = new Expense() { Id = 1, Description = "This is a test description.", Currency = "USD", Amount = 100.00M };
         mockRepository.Setup(x => x.CreateAsync(It.IsAny<Expense>())).Returns(Task.FromResult<Expense?>(mockExpense));
         var createExpense =
            new CreateExpenseRequest()
            {
               Description = mockExpense.Description,
               Currency = mockExpense.Currency,
               Amount = mockExpense.Amount
            };

         var useCase = new CreateExpense(mockRepository.Object);
         var response = await useCase.Handle(createExpense, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Id > 0);
      }
   }
}