using System;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Moq;
using Tests.Regression;

namespace Tests.Infrastructure.Adapters
{
    public class RepositoryAdapterTests
    {
        #region User

        [Theory]
        [AutoMoq]
        public async Task UserAdapter_Create(
            [Frozen] Mock<IDatabasePort<UserEntity>> mockUserRepo)
        {
            // Arrange
            var user =
               new User()
               {
                   Id = 1,
                   Name = "User One",
                   Phone = "+16023334578",
                   Email = "test@email.com",
                   UniqueKey = Guid.NewGuid()
               };

            var userEntity = DatabaseMapper.UserMapper.Map<UserEntity>(user);
            mockUserRepo.Setup(x => x.CreateAsync(It.IsAny<UserEntity>())).ReturnsAsync(userEntity);

            var repositoryAdapter = new RepositoryAdapter();

            // Act
            var dbUser = await repositoryAdapter.CreateAsync(user);

            // Assert
            mockUserRepo.Verify(repo => repo.CreateAsync(
               It.Is<UserEntity>(u =>
                    u.Id == dbUser!.Id &&
                    dbUser!.Name.Contains(u.FirstName) &&
                    dbUser!.Name.Contains(u.LastName) &&
                    u.Email == dbUser!.Email &&
                    u.Phone == dbUser!.Phone &&
                    u.UniqueKey == dbUser!.UniqueKey
               ))
            );
        }

        [Theory]
        [MemberData(nameof(UserData))]
        public async Task UserAdapter_Retrieve(
            [Frozen] Mock<IDatabasePort<UserEntity>> mockUserRepo,
            User user)
        {
            // Arrange
            var repositoryAdapter = new RepositoryAdapter();

            // Act
            var dbUser = await repositoryAdapter.RetrieveAsync(user);

            // Assert
            mockUserRepo.Verify(repo => repo.RetrieveAsync(
               It.Is<UserEntity>(u =>
                    u.Id == user.Id &&
                    user.Name.Contains(u.FirstName) &&
                    user.Name.Contains(u.LastName) &&
                    u.Email == user.Email &&
                    u.Phone == user.Phone &&
                    u.UniqueKey == user.UniqueKey
               ))
            );
        }

        [Theory]
        [MemberData(nameof(UserData))]
        public async Task UserAdapter_Update(
            [Frozen] Mock<IDatabasePort<UserEntity>> mockUserRepo,
            User user)
        {
            // Arrange
            var repositoryAdapter = new RepositoryAdapter();
            mockUserRepo.Setup(x => x.RetrieveAsync(It.IsAny<UserEntity>())).ReturnsAsync(DatabaseMapper.UserMapper.Map<UserEntity>(user));

            // Act
            var dbUser = await repositoryAdapter.UpdateAsync(user);

            // Assert
            mockUserRepo.Verify(repo => repo.UpdateAsync(
               It.Is<UserEntity>(u =>
                    u.Id == user.Id &&
                    user.Name.Contains(u.FirstName) &&
                    user.Name.Contains(u.LastName) &&
                    u.Email == user.Email &&
                    u.Phone == user.Phone &&
                    u.UniqueKey == user.UniqueKey
               ))
            );
        }

        [Theory]
        [MemberData(nameof(UserData))]
        public async Task UserAdapter_Delete(
            [Frozen] Mock<IDatabasePort<UserEntity>> mockUserRepo,
            User user)
        {
            // Arrange
            var repositoryAdapter = new RepositoryAdapter();

            // Act
            var dbUser = await repositoryAdapter.DeleteAsync(user);

            // Assert
            mockUserRepo.Verify(repo => repo.DeleteAsync(
               It.Is<UserEntity>(u =>
                    u.Id == user.Id &&
                    user.Name.Contains(u.FirstName) &&
                    user.Name.Contains(u.LastName) &&
                    u.Email == user.Email &&
                    u.Phone == user.Phone &&
                    u.UniqueKey == user.UniqueKey
               ))
            );
        }

        public static IEnumerable<object[]> UserData =>
           new List<object[]>()
           {
                new object[] { new User() { Id = 1, Name = "User One", Phone = "+16023334578", Email = "test@email.com", UniqueKey = Guid.NewGuid() } },
                new object[] { new User() { Id = 2, Name = "User Number Two", Phone = "+16023478562", Email = "random@email.com", UniqueKey = Guid.NewGuid() } },
                new object[] { new User() { Id = 3, Name = "SampleUser", Phone = "+6025558542", Email = "unique@email.com", UniqueKey = Guid.NewGuid() } }
           };

        #endregion

        #region Group

        // [Theory]
        // [MemberData(nameof(GroupData))]
        // public async Task GroupAdapter_Create(Group sampleGroup)
        // {

        // }

        public static IEnumerable<object[]> GroupData =>
           new List<object[]>()
           {
            new object[] { new Group() { Id = 1, Name = "Sample Group #1", UniqueKey = Guid.NewGuid() } }
           };

        #endregion
    }
}
