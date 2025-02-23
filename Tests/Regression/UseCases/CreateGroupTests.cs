using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class CreateGroupTests
   {
      [Fact]
      public async Task CreateGroupMoq()
      {
         var mockGroup = new Group() { Id = 1000, OwnerId = 1000, Name = "Initial Test Group" };
         var mockRepository = new Mock<ISqlDatabase<Group>>();
         mockRepository.Setup(x => x.Create(It.IsAny<Group>())).Returns(Task.FromResult<Group?>(mockGroup));
         var createGroup =
            new CreateGroupRequest()
            {
               OwnerId = mockGroup.OwnerId,
               Name = mockGroup.Name
            };
         var handler = new CreateGroup(mockRepository.Object);
         var response = await handler.Handle(createGroup, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Id > 0);
      }
   }
}