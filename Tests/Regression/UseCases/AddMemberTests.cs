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
      }
   }
}