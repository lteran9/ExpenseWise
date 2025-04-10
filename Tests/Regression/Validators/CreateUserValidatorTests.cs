using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
   public class CreateUserValidatorTests
   {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
      [Theory]
      [AutoMoq]
      public async Task Validate_NameNotEmpty(
         [Frozen] Mock<IDatabasePort<User>> mockRepository,
         CreateUser useCase)
      {
         // Arrange
         var createUser =
            new CreateUserRequest()
            {
               Email = "test@email.com",
               Phone = "6023334578"
            };

         // Act
         var response = await useCase.Handle(createUser, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_EmailNotEmpty(
         [Frozen] Mock<IDatabasePort<User>> mockRepository,
         CreateUser useCase)
      {
         // Arrange
         var createUser =
            new CreateUserRequest()
            {
               Name = "Test User",
               Phone = "6023334578"
            };

         // Act
         var response = await useCase.Handle(createUser, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_PhoneNotEmpty(
         [Frozen] Mock<IDatabasePort<User>> mockRepository,
         CreateUser useCase)
      {
         // Arrange
         var createUser =
            new CreateUserRequest()
            {
               Name = "Test User",
               Email = "test@email.com"
            };

         // Act
         var response = await useCase.Handle(createUser, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
   }
}