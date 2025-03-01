using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteUserValidatorTests
   {
      [Fact]
      public async Task Validate_IdNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<User>>();
         var deleteUser =
            new DeleteUserRequest();
         var handler = new DeleteUser(mockRepository.Object);
         var response = await handler.Handle(deleteUser, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}