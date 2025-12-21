using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class DeleteGroupValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_GroupIsValid(
           [Frozen] Mock<IDatabasePort<Group>> mockRepository,
           DeleteGroup useCase)
        {
            // Arrange
            var createGroup =
               new DeleteGroupRequest()
               {
                   RequestingUserId = 1000
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
        public async Task Validate_RequestingUserIsValid(
           [Frozen] Mock<IDatabasePort<Group>> mockRepository,
           DeleteGroup useCase)
        {
            // Arrange
            var createGroup =
               new DeleteGroupRequest()
               {
                   GroupId = 1000
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
