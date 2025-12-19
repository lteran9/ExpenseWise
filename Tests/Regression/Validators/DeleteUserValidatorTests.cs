using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class DeleteUserValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
        [Theory]
        [AutoMoq]
        public async Task DeleteUser_Has_UserId(
           [Frozen] Mock<IDatabasePort<User>> mockRepository,
           DeleteUser useCase)
        {
            // Arrange
            var deleteUser =
               new DeleteUserRequest();

            // Act
            var response = await useCase.Handle(deleteUser, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.True(response.ValidationMessages?.Any() == true);
        }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
