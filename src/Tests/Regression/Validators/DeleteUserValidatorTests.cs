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
        [Theory]
        [AutoMoq]
        public async Task DeleteUser_RequiresActorKey(
           [Frozen] Mock<IUserRepository> mockRepository,
           DeleteUser useCase)
        {
            // Arrange
            var deleteUser =
               new DeleteUserRequest();

            // Act
            var response = await useCase.Handle(deleteUser, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.NotNull(response.ValidationMessages);
            Assert.Contains("An authenticated user is required.", response.ValidationMessages);
        }
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}
