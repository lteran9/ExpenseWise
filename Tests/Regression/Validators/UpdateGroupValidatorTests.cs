using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class UpdateGroupValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_GroupGuidIsPresent(
            [Frozen] Mock<IDatabasePort<Group>> mock,
            UpdateGroup useCase
        )
        {
            // Arrange
            var request =
                new UpdateGroupRequest()
                {
                    Name = "Test Tester",
                };

            // Act
            var response = await useCase.Handle(request, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_NameIsPresent(
            [Frozen] Mock<IDatabasePort<Group>> mock,
            UpdateGroup useCase
        )
        {
            // Arrange
            var request =
                new UpdateGroupRequest()
                {
                    UniqueKey = Guid.NewGuid()
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_NameIsLongerThanThreeCharacters(
            [Frozen] Mock<IDatabasePort<Group>> mock,
            UpdateGroup useCase
        )
        {
            // Arrange
            var request =
                new UpdateGroupRequest()
                {
                    UniqueKey = Guid.NewGuid(),
                    Name = "IO"
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_StartDateIsLessThanOrEqualToEndDate(
            [Frozen] Mock<IDatabasePort<Group>> mock,
            UpdateGroup useCase
        )
        {
            // Arrange
            var request =
                new UpdateGroupRequest()
                {
                    UniqueKey = Guid.NewGuid(),
                    Name = "Upcoming Trip",
                    StartDate = DateTime.Now.AddDays(10),
                    EndDate = DateTime.Now
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
