using System;
using AutoFixture.Xunit2;
using Moq;
using Application.UseCases.Ports;
using Core.Entities;
using Application.UseCases;

namespace Tests.Regression.Validators
{
    public class AuthenticateUserValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_EmailNotEmpty(
            [Frozen] Mock<IDatabasePort<User>> userRepository,
            [Frozen] Mock<IDatabasePort<Password>> passwordRepository,
            AuthenticateUser useCase
        )
        {
            // Arrange
            var authenticateUserRequest =
                new AuthenticateUserRequest()
                {
                    Email = "",
                    Password = "abc123"
                };

            // Act
            var response = await useCase.Handle(authenticateUserRequest, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages); Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_EmailIsValid(
            [Frozen] Mock<IDatabasePort<User>> userRepository,
            [Frozen] Mock<IDatabasePort<Password>> passwordRepository,
            AuthenticateUser useCase
        )
        {
            // Arrange
            var authenticateUserRequest =
                new AuthenticateUserRequest()
                {
                    Email = "thisisnotavalidemail",
                    Password = "abc123"
                };

            // Act
            var response = await useCase.Handle(authenticateUserRequest, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages); Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_PasswordNotEmpty(
            [Frozen] Mock<IDatabasePort<User>> userRepository,
            [Frozen] Mock<IDatabasePort<Password>> passwordRepository,
            AuthenticateUser useCase
        )
        {
            // Arrange
            var authenticateUserRequest =
                new AuthenticateUserRequest()
                {
                    Email = "valid@email.com",
                    Password = ""
                };

            // Act
            var response = await useCase.Handle(authenticateUserRequest, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
