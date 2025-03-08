using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteUserTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteUserMoq([Frozen] Mock<ISqlDatabase<User>> mockRepository)
      {
         var mockUser = new User() { Id = 1000 };
         mockRepository.Setup(x => x.Delete(It.IsAny<User>())).Returns(Task.FromResult<User?>(mockUser));
         var deleteUser =
            new DeleteUserRequest()
            {
               Id = mockUser.Id
            };
         var useCase = new DeleteUser(mockRepository.Object);
         var response = await useCase.Handle(deleteUser, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}