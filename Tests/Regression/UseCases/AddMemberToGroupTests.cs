using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class AddMemberToGroupTests
    {
        [Theory]
        [AutoMoq]
        public async Task AddMember_WithValidRequest_ShouldSucceed(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var groupKey = Guid.NewGuid();
            var user =
                new User
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US"
                };
            var group =
                new Group
                {
                    UniqueKey = groupKey
                };

            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = groupKey
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync(group);
            mockGroupRepository.Setup(r => r.AddMemberAsync(It.IsAny<MemberOf>()))
                .ReturnsAsync(new MemberOf());

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(response.Succeeded);
            mockGroupRepository.Verify(r => r.AddMemberAsync(It.IsAny<MemberOf>()), Times.Once);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WithInvalidPhoneNumber_ShouldFail(
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "invalid-phone",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WithEmptyGroupKey_ShouldFail(
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.Empty
                };

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WhenUserNotFound_ShouldFail(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            var group =
                new Group
                {
                    UniqueKey = request.GroupUniqueKey
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync(group);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WhenGroupNotFound_ShouldFail(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            var user =
                new User
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US"
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Group?)null);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WithCountryCodeMismatch_ShouldFail(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            var user =
                new User
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "MX"
                };
            var group =
                new Group
                {
                    UniqueKey = request.GroupUniqueKey
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync(group);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WhenRepositoryReturnsNull_ShouldFail(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            var user =
                new User
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US"
                };
            var group =
                new Group
                {
                    UniqueKey = request.GroupUniqueKey
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync(group);
            mockGroupRepository.Setup(r => r.AddMemberAsync(It.IsAny<MemberOf>()))
                .ReturnsAsync((MemberOf?)null);

            // Act
            var response = await useCase.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WhenRepositoryThrows_ShouldFail(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            // Arrange
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid()
                };

            var user =
                new User
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US"
                };
            var group =
                new Group
                {
                    UniqueKey = request.GroupUniqueKey
                };

            mockUserRepository.Setup(r => r.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(r => r.FindByUniqueKeyAsync(It.IsAny<Guid>()))
                .ReturnsAsync(group);
            mockGroupRepository.Setup(r => r.AddMemberAsync(It.IsAny<MemberOf>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => useCase.Handle(request, CancellationToken.None));
        }
    }
}
