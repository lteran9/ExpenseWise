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
           [Frozen] Mock<IGroupRepository> mockRepository,
           DeleteMember useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.RemoveMemberAsync(It.IsAny<MemberOf>())).ReturnsAsync(new MemberOf() { Id = 1000 });
            var groupKey = Guid.NewGuid();
            var actorKey = Guid.NewGuid();
            var memberKey = Guid.NewGuid();
            mockRepository.Setup(x => x.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    Id = 10,
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = actorKey },
                    Members = [new User { Id = 20, UniqueKey = memberKey }]
                });
            var deleteMember =
               new DeleteMemberRequest()
               {
                   GroupKey = groupKey,
                   MemberKey = memberKey,
                   ActorKey = actorKey
               };

            // Act
            var response = await useCase.Handle(deleteMember, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Success);
            mockRepository.Verify(repository => repository.RemoveMemberAsync(It.Is<MemberOf>(
                membership => membership.Group.UniqueKey == groupKey &&
                    membership.User.UniqueKey == memberKey)), Times.Once);
        }

        [Fact]
        public async Task DeleteMember_WhenActorIsNotOrganizer_DoesNotRemoveMember()
        {
            var groupKey = Guid.NewGuid();
            var actorKey = Guid.NewGuid();
            var memberKey = Guid.NewGuid();
            var repository = new Mock<IGroupRepository>();
            repository.Setup(value => value.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = Guid.NewGuid() },
                    Members = [new User { UniqueKey = memberKey }]
                });
            var useCase = new DeleteMember(repository.Object);

            var response = await useCase.Handle(new DeleteMemberRequest
            {
                GroupKey = groupKey,
                MemberKey = memberKey,
                ActorKey = actorKey
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Only the group organizer", response.ValidationMessages!.Single());
            repository.Verify(value => value.RemoveMemberAsync(It.IsAny<MemberOf>()), Times.Never);
        }
    }
}
