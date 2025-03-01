using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteUserTests
   {
      [Fact]
      public async Task DeleteUserMoq()
      {
         var mockUser = new User() { Id = 1000 };
         var mockRepository = new Mock<ISqlDatabase<User>>();
         mockRepository.Setup(x => x.Delete(It.IsAny<User>())).Returns(Task.FromResult<User?>(mockUser));
         var createUser =
            new DeleteUserRequest()
            {
               Id = mockUser.Id
            };
         var useCase = new DeleteUser(mockRepository.Object);
         var response = await useCase.Handle(createUser, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}