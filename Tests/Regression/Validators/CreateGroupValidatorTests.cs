using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateGroupValidatorTests
   {
      [Fact]
      public async Task Validate_NameNotEmpty()
      {
         var mockRepository = new Mock<ISqlDatabase<Group>>();
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

      [Fact]
      public async Task Validate_OwnerIdPresent()
      {
         var mockRepository = new Mock<ISqlDatabase<Group>>();
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