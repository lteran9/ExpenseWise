using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class CreateGroupTests
    {
        [Theory]
        [AutoMoq]
        public async Task CreateGroupMoq(
           [Frozen] Mock<IGroupRepository> mockRepository,
           CreateGroup useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.CreateWithOwnerMembershipAsync(It.IsAny<Group>()))
                .ReturnsAsync(new Group() { Id = 1, UniqueKey = Guid.NewGuid() });
            var createGroup =
               new CreateGroupRequest()
               {
                   ActorKey = Guid.NewGuid(),
                   Name = "Initial Test Group"
               };

            // Act
            var response = await useCase.Handle(createGroup, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Id > 0);
            mockRepository.Verify(x => x.CreateWithOwnerMembershipAsync(
                It.Is<Group>(group => group.Owner.UniqueKey == createGroup.ActorKey)), Times.Once);
            mockRepository.Verify(x => x.AddMemberAsync(It.IsAny<MemberOf>()), Times.Never);
        }
    }
}
