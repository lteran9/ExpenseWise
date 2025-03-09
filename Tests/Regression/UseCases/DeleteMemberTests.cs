using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteMemberTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteMemberMoq([Frozen] Mock<ISqlDatabase<MemberOf>> mockRepository)
      {
         var mockUser = new MemberOf() { Id = 1000 };
         mockRepository.Setup(x => x.DeleteAsync(It.IsAny<MemberOf>())).Returns(Task.FromResult<MemberOf?>(mockUser));
         var deleteMember =
            new DeleteMemberRequest()
            {
               Id = mockUser.Id
            };
         var useCase = new DeleteMember(mockRepository.Object);
         var response = await useCase.Handle(deleteMember, new CancellationToken());

         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}