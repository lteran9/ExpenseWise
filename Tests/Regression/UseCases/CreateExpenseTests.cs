using System;
using System.Net.Cache;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateExpenseTests
   {
      [Fact]
      public async Task CreateExpenseMoq()
      {
         var mockExpense = new Expense() { Id = 1, Description = "This is a test description.", Currency = "USD", Amount = 100.00M };
         var mockRepository = new Mock<ISqlDatabase<Expense>>();
         mockRepository.Setup(x => x.Create(It.IsAny<Expense>())).Returns(Task.FromResult<Expense?>(mockExpense));
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