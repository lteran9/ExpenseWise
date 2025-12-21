using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class CreateUserValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        [Theory]
        [AutoMoq]
        public async Task Validate_NameNotEmpty(
           [Frozen] Mock<IDatabasePort<User>> mockRepository,
           CreateUser useCase)
        {
            // Arrange
            var createUser =
               new CreateUserRequest()
               {
                   Email = "test@email.com",
                   Phone = "6023334578"
               };

            // Act
            var response = await useCase.Handle(createUser, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_EmailNotEmpty(
           [Frozen] Mock<IDatabasePort<User>> mockRepository,
           CreateUser useCase)
        {
            // Arrange
            var createUser =
               new CreateUserRequest()
               {
                   Name = "Test User",
                   Phone = "6023334578"
               };

            // Act
            var response = await useCase.Handle(createUser, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_PhoneNotEmpty(
           [Frozen] Mock<IDatabasePort<User>> mockRepository,
           CreateUser useCase)
        {
            // Arrange
            var createUser =
               new CreateUserRequest()
               {
                   Name = "Test User",
                   Email = "test@email.com"
               };

            // Act
            var response = await useCase.Handle(createUser, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.NotEmpty(response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
