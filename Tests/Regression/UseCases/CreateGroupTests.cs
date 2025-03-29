using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateGroupTests
   {
      [Theory]
      [AutoMoq]
      public async Task CreateGroupMoq(
         [Frozen] Mock<ISqlDatabase<Group>> mockRepository,
         CreateGroup useCase)
      {
         // Arrange
         var mockGroup = new Group() { Id = 1000, Owner = new User() { Id = 1000 }, Name = "Initial Test Group" };
         mockRepository.Setup(x => x.CreateAsync(It.IsAny<Group>())).ReturnsAsync(mockGroup);
         var createGroup =
            new CreateGroupRequest()
            {
               OwnerId = mockGroup.Owner.Id,
               Name = mockGroup.Name
            };

         // Act
         var response = await useCase.Handle(createGroup, new CancellationToken());

         // Assert
         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Id > 0);
      }
   }
}