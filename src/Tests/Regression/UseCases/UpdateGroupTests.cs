using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class UpdateGroupTests
    {
        [Theory]
        [AutoMoq]
        public async Task UpdateUserMoq(
                   [Frozen] Mock<IGroupRepository> mockRepository,
                   UpdateGroup useCase)
        {
            // Arrange
            var groupKey = Guid.NewGuid();
            var actorKey = Guid.NewGuid();
            mockRepository.Setup(x => x.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group { UniqueKey = groupKey, Owner = new User { UniqueKey = actorKey } });
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Group>())).ReturnsAsync(new Group() { Id = 1000 });
            var updateGroup =
               new UpdateGroupRequest()
               {
                   UniqueKey = groupKey,
                   ActorKey = actorKey,
                   Name = "Standard Group Bills",
                   Active = false,
                   StartDate = DateTime.Today.AddDays(10),
                   EndDate = DateTime.Today.AddDays(20)
               };

            // Act
            var response = await useCase.Handle(updateGroup, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.NotNull(response.Result);
            Assert.True(response.Result.Success);
        }

        [Fact]
        public async Task UpdateGroup_WhenActorIsNotOrganizer_DoesNotUpdate()
        {
            var groupKey = Guid.NewGuid();
            var actorKey = Guid.NewGuid();
            var repository = new Mock<IGroupRepository>();
            repository.Setup(value => value.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = Guid.NewGuid() }
                });
            var useCase = new UpdateGroup(repository.Object);

            var response = await useCase.Handle(new UpdateGroupRequest
            {
                UniqueKey = groupKey,
                ActorKey = actorKey,
                Name = "Updated Group",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            repository.Verify(value => value.UpdateAsync(It.IsAny<Group>()), Times.Never);
        }
    }
}
