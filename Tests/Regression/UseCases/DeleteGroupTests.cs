using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
   public class DeleteGroupTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteGroupMoq(
         [Frozen] Mock<IDatabasePort<Group>> mockRepository,
         DeleteGroup useCase)
      {
         // Arrange
         mockRepository.Setup(x => x.RetrieveAsync(It.IsAny<Group>())).ReturnsAsync(new Group() { Id = 1000, Owner = new User() { Id = 1000 } });
         var deleteGroup =
            new DeleteGroupRequest()
            {
               GroupId = 1000,
               RequestingUserId = 1000
            };

         // Act
         var response = await useCase.Handle(deleteGroup, new CancellationToken());

         // Assert
         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}