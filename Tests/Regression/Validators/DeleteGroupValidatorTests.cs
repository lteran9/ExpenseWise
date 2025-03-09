using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteGroupValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_GroupIsValid([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var createGroup =
            new DeleteGroupRequest()
            {
               RequestingUserId = 1000
            };
         var handler = new DeleteGroup(mockRepository.Object);
         var response = await handler.Handle(createGroup, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_RequestingUserIsValid([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var createGroup =
            new DeleteGroupRequest()
            {
               GroupId = 1000
            };
         var handler = new DeleteGroup(mockRepository.Object);
         var response = await handler.Handle(createGroup, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}