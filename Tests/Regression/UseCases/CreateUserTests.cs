using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateUserTests
   {
      [Fact]
      public async Task CreateUserMoq()
      {
         var mockRepository = new Mock<ISqlDatabase<User>>();
         mockRepository.Setup(x => x.Create(It.IsAny<User>())).Returns(Task.FromResult(new User()));
         var createUser =
            new CreateUserRequest()
            {
               Email = "test@email.com",
               Phone = "6023334578"
            };
         var handler = new CreateUser(mockRepository.Object);
         var response = await handler.Handle(createUser, new CancellationToken());
      }
   }
}