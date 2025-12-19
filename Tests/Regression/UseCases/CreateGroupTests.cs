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
           [Frozen] Mock<IDatabasePort<Group>> mockRepository,
           CreateGroup useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.CreateAsync(It.IsAny<Group>())).ReturnsAsync(new Group() { Id = 1 });
            var createGroup =
               new CreateGroupRequest()
               {
                   OwnerId = 1000,
                   Name = "Initial Test Group"
               };

            // Act
            var response = await useCase.Handle(createGroup, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Id > 0);
        }
    }
}
