using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
   public class DeleteUserTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteUserMoq(
         [Frozen] Mock<IDatabasePort<User>> mockRepository,
         DeleteUser useCase)
      {
         // Arrange
         mockRepository.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(new User() { Id = 1000 });
         var deleteUser =
            new DeleteUserRequest()
            {
               Id = 1000
            };

         // Act
         var response = await useCase.Handle(deleteUser, new CancellationToken());

         // Assert
         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}