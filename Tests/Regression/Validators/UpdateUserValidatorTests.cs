using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class UpdateUserValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_UserGuidIsPresent(
            [Frozen] Mock<IDatabasePort<User>> mock,
            UpdateUser useCase
        )
        {
            // Arrange
            var request =
                new UpdateUserRequest()
                {
                    Name = "Test Tester",
                    Phone = "+1 (602) 333-4578",
                    Email = "test@user.com"
                };

            // Act
            var response = await useCase.Handle(request, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
