using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
   public class DeleteGroupValidatorTests
   {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
      [Theory]
      [AutoMoq]
      public async Task Validate_GroupIsValid(
         [Frozen] Mock<IDatabasePort<Group>> mockRepository,
         DeleteGroup useCase)
      {
         // Arrange
         var createGroup =
            new DeleteGroupRequest()
            {
               RequestingUserId = 1000
            };

         // Act
         var response = await useCase.Handle(createGroup, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_RequestingUserIsValid(
         [Frozen] Mock<IDatabasePort<Group>> mockRepository,
         DeleteGroup useCase)
      {
         // Arrange
         var createGroup =
            new DeleteGroupRequest()
            {
               GroupId = 1000
            };

         // Act
         var response = await useCase.Handle(createGroup, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
   }
}