using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateGroupValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_NameNotEmpty([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var createGroup =
            new CreateGroupRequest()
            {
               OwnerId = 1000
            };
         var handler = new CreateGroup(mockRepository.Object);
         var response = await handler.Handle(createGroup, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_OwnerIdPresent([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var createGroup =
            new CreateGroupRequest()
            {
               Name = "Test Group"
            };
         var handler = new CreateGroup(mockRepository.Object);
         var response = await handler.Handle(createGroup, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}