using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
   public class DeleteMemberTests
   {
      [Theory]
      [AutoMoq]
      public async Task DeleteMemberMoq(
         [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
         DeleteMember useCase)
      {
         // Arrange
         mockRepository.Setup(x => x.RetrieveAsync(It.IsAny<MemberOf>())).ReturnsAsync(new MemberOf() { Id = 1000 });
         var deleteMember =
            new DeleteMemberRequest()
            {
               Id = 1000
            };

         // Act
         var response = await useCase.Handle(deleteMember, new CancellationToken());

         // Assert
         Assert.True(response.Succeeded);
         Assert.True(response.Result != null);
         Assert.True(response.Result.Success);
      }
   }
}