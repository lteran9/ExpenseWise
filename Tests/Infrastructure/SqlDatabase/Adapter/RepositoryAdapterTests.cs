using System;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Moq;

namespace Tests.Infrastructure.Adapters
{
   public class RepositoryAdapterTests
   {
      private readonly Mock<ISqlDatabase<UserEntity>> _mockUserRepo;
      private readonly Mock<ISqlDatabase<GroupEntity>> _mockGroupRepo;

      public RepositoryAdapterTests()
      {
         _mockUserRepo = new Mock<ISqlDatabase<UserEntity>>();
         _mockGroupRepo = new Mock<ISqlDatabase<GroupEntity>>();
      }

      #region User

      [Theory]
      [MemberData(nameof(UserData))]
      public async Task UserAdapter_Create(User user)
      {
         // Arrange
         var repositoryAdapter =
            new RepositoryAdapter(_mockUserRepo.Object, _mockGroupRepo.Object);

         // Act
         var dbUser = await repositoryAdapter.CreateAsync(user);

         // Assert
         _mockUserRepo.Verify(repo => repo.CreateAsync(
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
      public async Task UserAdapter_Retrieve(User user)
      {
         // Arrange
         var repositoryAdapter =
            new RepositoryAdapter(_mockUserRepo.Object, _mockGroupRepo.Object);

         // Act
         var dbUser = await repositoryAdapter.GetAsync(user);

         // Assert
         _mockUserRepo.Verify(repo => repo.GetAsync(
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
      public async Task UserAdapter_Update(User user)
      {
         // Arrange
         var repositoryAdapter =
            new RepositoryAdapter(_mockUserRepo.Object, _mockGroupRepo.Object);
         _mockUserRepo.Setup(x => x.GetAsync(It.IsAny<UserEntity>())).ReturnsAsync(DatabaseMapper.UserMapper.Map<UserEntity>(user));

         // Act
         var dbUser = await repositoryAdapter.UpdateAsync(user);

         // Assert
         _mockUserRepo.Verify(repo => repo.UpdateAsync(
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
      public async Task UserAdapter_Delete(User user)
      {
         // Arrange
         var repositoryAdapter =
            new RepositoryAdapter(_mockUserRepo.Object, _mockGroupRepo.Object);

         // Act
         var dbUser = await repositoryAdapter.DeleteAsync(user);

         // Assert
         _mockUserRepo.Verify(repo => repo.DeleteAsync(
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