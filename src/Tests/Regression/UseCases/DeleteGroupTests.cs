using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class DeleteGroupTests
    {
        [Theory]
        [AutoMoq]
        public async Task DeleteGroupMoq(
           [Frozen] Mock<IGroupRepository> mockRepository,
           DeleteGroup useCase)
        {
            // Arrange
            var groupKey = Guid.NewGuid();
            var actorKey = Guid.NewGuid();
            mockRepository.Setup(x => x.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group { UniqueKey = groupKey, Owner = new User { UniqueKey = actorKey } });
            mockRepository.Setup(x => x.DeleteAsync(It.IsAny<Group>())).ReturnsAsync(new Group { Id = 1000 });
            var deleteGroup =
               new DeleteGroupRequest()
               {
                   GroupKey = groupKey,
                   ActorKey = actorKey
               };

            // Act
            var response = await useCase.Handle(deleteGroup, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Success);
        }

        [Fact]
        public async Task DeleteGroup_WithEmptyActorKey_ReturnsValidationFailure()
        {
            var repository = new Mock<IGroupRepository>();
            var useCase = new DeleteGroup(repository.Object);

            var response = await useCase.Handle(new DeleteGroupRequest
            {
                GroupKey = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("An authenticated user is required.", response.ValidationMessages!);
            repository.Verify(value => value.FindByUniqueKeyAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
