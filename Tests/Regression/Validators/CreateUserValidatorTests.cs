using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateUserValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_NameNotEmpty([Frozen] Mock<ISqlDatabase<User>> mockRepository)
      {
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

      [Theory]
      [AutoMoq]
      public async Task Validate_EmailNotEmpty([Frozen] Mock<ISqlDatabase<User>> mockRepository)
      {
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

      [Theory]
      [AutoMoq]
      public async Task Validate_PhoneNotEmpty([Frozen] Mock<ISqlDatabase<User>> mockRepository)
      {
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