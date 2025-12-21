using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class CreateGroupValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_NameNotEmpty(
           [Frozen] Mock<IDatabasePort<Group>> mockRepository,
           CreateGroup useCase)
        {
            // Arrange
            var createGroup =
               new CreateGroupRequest()
               {
                   OwnerId = 1000
               };

            // Act
            var response = await useCase.Handle(createGroup, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_OwnerIdPresent(
           [Frozen] Mock<IDatabasePort<Group>> mockRepository,
           CreateGroup useCase)
        {
            // Arrange
            var createGroup =
               new CreateGroupRequest()
               {
                   Name = "Test Group"
               };

            // Act
            var response = await useCase.Handle(createGroup, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
