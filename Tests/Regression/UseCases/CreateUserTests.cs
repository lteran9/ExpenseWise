using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class CreateUserTests
    {
        [Theory]
        [AutoMoq]
        public async Task CreateUserMoq(
           [Frozen] Mock<IDatabasePort<User>> mockRepository,
           CreateUser useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User() { Id = 1000 });
            var createUser =
               new CreateUserRequest()
               {
                   Name = "Test Tester",
                   Email = "test@example.com",
                   Phone = "+16023334578",
                   Password = "HZAm69_drGeLMDU4dAez",
                   ConfirmPassword = "HZAm69_drGeLMDU4dAez"
               };

            // Act
            var response = await useCase.Handle(createUser, new CancellationToken());

            // Assert
            Assert.True(response.Succeeded);
            Assert.True(response.Result != null);
            Assert.True(response.Result.Id > 0);
        }
    }
}