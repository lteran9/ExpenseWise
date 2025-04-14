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
      [Theory]
      [AutoMoq]
      public async Task CreateExpenseMoq(
         [Frozen] Mock<IDatabasePort<Expense>> mockRepository,
         CreateExpense useCase)
      {
         // Arrange
         mockRepository.Setup(x => x.CreateAsync(It.IsAny<Expense>())).ReturnsAsync(new Expense() { Id = 1 });
         var createExpense =
            new CreateExpenseRequest()
            {
               Description = "This is a test description.",
               Currency = "USD",
               Amount = 100.00M
            };

         // Act
         var response = await useCase.Handle(createExpense, new CancellationToken());

         // Assert
         mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<Expense>()));

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Id > 0);
      }
   }
}