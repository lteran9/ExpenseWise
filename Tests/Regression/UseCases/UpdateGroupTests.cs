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
                   [Frozen] Mock<IDatabasePort<Group>> mockRepository,
                   UpdateGroup useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Group>())).ReturnsAsync(new Group() { Id = 1000 });
            var updateGroup =
               new UpdateGroupRequest()
               {
                   UniqueKey = Guid.NewGuid(),
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
    }
}
