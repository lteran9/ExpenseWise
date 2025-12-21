using System;
using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using AutoFixture.Xunit2;
using Moq;
using Xunit;

namespace Tests.Regression.Validators
{
    public class FindUserValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_UserGuidIsPresent(
            [Frozen] Mock<IDatabasePort<User>> mock,
            FindUser useCase
        )
        {
            // Arrange
            var request =
                new FindUserRequest()
                {

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
