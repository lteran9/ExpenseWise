using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteGroupTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteGroupMoq([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var mockUser = new User() { Id = 1000 };
         var mockGroup = new Group() { Id = 1000, Owner = mockUser, Name = "Initial Test Group" };
         mockRepository.Setup(x => x.GetAsync(It.IsAny<Group>())).Returns(Task.FromResult<Group?>(mockGroup));
         mockRepository.Setup(x => x.DeleteAsync(It.IsAny<Group>())).Returns(Task.FromResult<Group?>(mockGroup));
         var createGroup =
            new DeleteGroupRequest()
            {
               GroupId = 1000,
               RequestingUserId = 1000
            };
         var useCase = new DeleteGroup(mockRepository.Object);
         var response = await useCase.Handle(createGroup, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }

      [Theory]
      [AutoMoq]
      public async Task DeleteGroupMoq_OnlyByOwner([Frozen] Mock<ISqlDatabase<Group>> mockRepository)
      {
         var mockUser = new User() { Id = 1000 };
         var mockGroup = new Group() { Id = 1000, Owner = mockUser, Name = "Initial Test Group" };
         mockRepository.Setup(x => x.GetAsync(It.IsAny<Group>())).Returns(Task.FromResult<Group?>(mockGroup));
         mockRepository.Setup(x => x.DeleteAsync(It.IsAny<Group>())).Returns(Task.FromResult<Group?>(mockGroup));
         var createGroup =
            new DeleteGroupRequest()
            {
               GroupId = 1000,
               RequestingUserId = 1001
            };
         var useCase = new DeleteGroup(mockRepository.Object);
         var response = await useCase.Handle(createGroup, new CancellationToken());

         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}