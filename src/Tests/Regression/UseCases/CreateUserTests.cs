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
           [Frozen] Mock<IUserRepository> mockRepository,
           [Frozen] Mock<IPasswordRepository> mockPasswordRepository,
           CreateUser useCase)
        {
            // Arrange
            mockRepository.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            mockRepository.Setup(x => x.FindByCanonicalPhoneAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            mockRepository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User() { Id = 1000 });
            mockPasswordRepository.Setup(x => x.CreateAsync(It.IsAny<Password>())).ReturnsAsync(new Password());
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
            mockRepository.Verify(x => x.FindByCanonicalPhoneAsync(createUser.Phone), Times.Once);
        }

        [Fact]
        public async Task CreateUser_WhenEmailAlreadyExists_ReturnsFriendlyValidationError()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(new User { Email = "existing@example.com" });
            var useCase = new CreateUser(
                userRepository.Object,
                Mock.Of<IPasswordRepository>());

            var response = await useCase.Handle(new CreateUserRequest
            {
                Name = "Test User",
                Email = "existing@example.com",
                Phone = "6023334567",
                CountryCode = "+1",
                Password = "ValidPass123!",
                ConfirmPassword = "ValidPass123!"
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("email address", response.ValidationMessages!.Single());
            userRepository.Verify(repository => repository.CreateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
