using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
   public class AddMemberValidatorTests
   {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
      [Theory]
      [AutoMoq]
      public async Task Validate_UserNotNull(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         AddMember useCase)
      {
         // Arrange
         var addMember =
            new AddMemberRequest()
            {
               Group = new Group() { Id = 1000 }
            };

         // Act
         var response = await useCase.Handle(addMember, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_UserIsValid(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         AddMember useCase)
      {
         // Arrange
         var addMember =
            new AddMemberRequest()
            {
               User = new User(),
               Group = new Group() { Id = 1000 }
            };

         // Act
         var response = await useCase.Handle(addMember, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_GroupNotNull(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         AddMember useCase)
      {
         // Arrange
         var addMember =
            new AddMemberRequest()
            {
               User = new User() { Id = 1000 }
            };

         // Act
         var response = await useCase.Handle(addMember, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_GroupIsValid(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         AddMember useCase)
      {
         // Arrange
         var addMember =
            new AddMemberRequest()
            {
               User = new User() { Id = 1000 },
               Group = new Group()
            };

         // Act
         var response = await useCase.Handle(addMember, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
   }
}