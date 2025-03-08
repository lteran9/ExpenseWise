using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class DeleteMemberValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_IdNotEmpty([Frozen] Mock<ISqlDatabase<MemberOf>> mockRepository)
      {
         var deleteMember =
            new DeleteMemberRequest();
         var handler = new DeleteMember(mockRepository.Object);
         var response = await handler.Handle(deleteMember, new CancellationToken());
         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}