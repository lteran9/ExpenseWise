using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateUserValidatorTests
   {
      [Fact]
      public async Task Validate_NameNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<User>>();
         var createUser =
            new CreateUserRequest()
            {
               Email = "test@email.com",
               Phone = "6023334578"
            };
         var handler = new CreateUser(mockRepository.Object);
         var response = await handler.Handle(createUser, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Fact]
      public async Task Validate_EmailNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<User>>();
         var createUser =
            new CreateUserRequest()
            {
               Name = "Test User",
               Phone = "6023334578"
            };
         var handler = new CreateUser(mockRepository.Object);
         var response = await handler.Handle(createUser, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Fact]
      public async Task Validate_PhoneNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<User>>();
         var createUser =
            new CreateUserRequest()
            {
               Name = "Test User",
               Email = "test@email.com"
            };
         var handler = new CreateUser(mockRepository.Object);
         var response = await handler.Handle(createUser, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}