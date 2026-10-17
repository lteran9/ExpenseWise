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
            var organizerKey = Guid.NewGuid();
            var user =
                new User
                {
                    UniqueKey = Guid.NewGuid(),
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US"
                };
            var group =
                new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = organizerKey }
                };

            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = groupKey,
                    ActorKey = organizerKey
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
        public async Task AddMember_WhenActorIsNotOrganizer_ShouldBeRejected(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            var groupKey = Guid.NewGuid();
            var user = new User
            {
                UniqueKey = Guid.NewGuid(),
                CountryCode = "US",
                Phone = "+1 (602) 333-4567"
            };
            mockUserRepository.Setup(repository => repository.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = Guid.NewGuid() }
                });

            var response = await useCase.Handle(new AddMemberToGroupRequest
            {
                GroupUniqueKey = groupKey,
                ActorKey = Guid.NewGuid(),
                Phone = user.Phone,
                CountryCode = user.CountryCode
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            mockGroupRepository.Verify(repository => repository.AddMemberAsync(It.IsAny<MemberOf>()), Times.Never);
        }

        [Theory]
        [AutoMoq]
        public async Task AddMember_WhenUserIsAlreadyMember_ShouldBeRejected(
            [Frozen] Mock<IUserRepository> mockUserRepository,
            [Frozen] Mock<IGroupRepository> mockGroupRepository,
            AddMemberToGroup useCase)
        {
            var groupKey = Guid.NewGuid();
            var organizerKey = Guid.NewGuid();
            var user = new User
            {
                UniqueKey = Guid.NewGuid(),
                CountryCode = "US",
                Phone = "+1 (602) 333-4567"
            };
            mockUserRepository.Setup(repository => repository.FindByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockGroupRepository.Setup(repository => repository.FindByUniqueKeyAsync(groupKey))
                .ReturnsAsync(new Group
                {
                    UniqueKey = groupKey,
                    Owner = new User { UniqueKey = organizerKey },
                    Members = [user]
                });

            var response = await useCase.Handle(new AddMemberToGroupRequest
            {
                GroupUniqueKey = groupKey,
                ActorKey = organizerKey,
                Phone = user.Phone,
                CountryCode = user.CountryCode
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            mockGroupRepository.Verify(repository => repository.AddMemberAsync(It.IsAny<MemberOf>()), Times.Never);
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
        public async Task AddMember_WithEmptyActorKey_ShouldFail(
            AddMemberToGroup useCase)
        {
            var response = await useCase.Handle(new AddMemberToGroupRequest
            {
                Phone = "+1 (602) 333-4567",
                CountryCode = "US",
                GroupUniqueKey = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("An authenticated user is required.", response.ValidationMessages!);
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
                    GroupUniqueKey = Guid.NewGuid(),
                    ActorKey = Guid.NewGuid()
                };

            var group =
                new Group
                {
                    UniqueKey = request.GroupUniqueKey,
                    Owner = new User { UniqueKey = request.ActorKey }
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
                    GroupUniqueKey = Guid.NewGuid(),
                    ActorKey = Guid.NewGuid()
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
                    UniqueKey = request.GroupUniqueKey,
                    Owner = new User { UniqueKey = request.ActorKey }
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
            var actorKey = Guid.NewGuid();
            var request =
                new AddMemberToGroupRequest
                {
                    Phone = "+1 (602) 333-4567",
                    CountryCode = "US",
                    GroupUniqueKey = Guid.NewGuid(),
                    ActorKey = actorKey
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
                    UniqueKey = request.GroupUniqueKey,
                    Owner = new User { UniqueKey = actorKey }
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
