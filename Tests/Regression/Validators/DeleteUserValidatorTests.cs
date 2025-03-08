using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteUserValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_IdNotEmpty([Frozen] Mock<ISqlDatabase<User>> mockRepository)
      {
         var deleteUser =
            new DeleteUserRequest();
         var handler = new DeleteUser(mockRepository.Object);
         var response = await handler.Handle(deleteUser, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}