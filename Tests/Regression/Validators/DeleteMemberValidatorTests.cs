using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
   public class DeleteMemberValidatorTests
   {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
      [Theory]
      [AutoMoq]
      public async Task DeleteMember_Has_UserId(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         DeleteMember useCase)
      {
         // Arrange
         var deleteMember =
            new DeleteMemberRequest();

         // Act
         var response = await useCase.Handle(deleteMember, new CancellationToken());

         // Assert
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
   }
}