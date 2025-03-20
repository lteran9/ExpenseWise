using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class AddMemberTests
   {
      [Theory]
      [AutoMoq]
      public async Task AddMemberMoq([Frozen] Mock<ISqlDatabase<MemberOf>> mockRepository)
      {
         var mockUser = new User() { Id = 1000 };
         var mockGroup = new Group() { Id = 1000 };
         var mockMembership =
            new MemberOf()
            {
               User = mockUser,
               Group = mockGroup
            };
         mockRepository.Setup(x => x.CreateAsync(It.IsAny<MemberOf>())).Returns(Task.FromResult<MemberOf?>(mockMembership));
         var addMemberRequest =
            new AddMemberRequest()
            {
               User = mockUser,
               Group = mockGroup
            };
         var useCase = new AddMember(mockRepository.Object);
         var response = await useCase.Handle(addMemberRequest, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}