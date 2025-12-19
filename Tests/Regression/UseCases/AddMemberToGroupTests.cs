using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class AddMemberTests
    {
        [Theory]
        [AutoMoq]
        public async Task AddMemberMoq(
           [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
           AddMemberToGroup useCase)
        {
            // Arrange
            var addMemberRequest =
               new AddMemberToGroupRequest()
               {
                   Phone = "+1 (602) 333-4567",
                   GroupUniqueKey = Guid.NewGuid()
               };

            // Act
            var response = await useCase.Handle(addMemberRequest, new CancellationToken());

            // Assert
            mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<MemberOf>()));

            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Success);
        }
    }
}
