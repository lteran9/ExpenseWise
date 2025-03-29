using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateUserTests
   {
      [Theory]
      [AutoMoq]
      public async Task CreateUserMoq(
         [Frozen] Mock<ISqlDatabase<User>> mockRepository,
         CreateUser useCase)
      {
         // Arrange
         var mockUser = new User() { Id = 1000, Name = "Test User", Email = "test@email.com", Phone = "6023334578" };
         mockRepository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(mockUser);
         var createUser =
            new CreateUserRequest()
            {
               Name = mockUser.Name,
               Email = mockUser.Email,
               Phone = mockUser.Phone
            };

         // Act
         var response = await useCase.Handle(createUser, new CancellationToken());

         // Assert
         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Id > 0);
      }
   }
}