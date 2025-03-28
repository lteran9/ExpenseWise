using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class RepositoryAdapterTests
   {
      private readonly IRepository _repositoryAdapter;

      public RepositoryAdapterTests()
      {
         _repositoryAdapter = new RepositoryAdapter();
      }

      #region User

      [Theory]
      [MemberData(nameof(UserData))]
      public async Task UserAdapter_Create(User testUser)
      {
         var dbUser = await _repositoryAdapter.CreateAsync(testUser);

         Assert.NotNull(dbUser);
         Assert.True(dbUser.Id > 0);
         Assert.Equal(testUser.Name, dbUser.Name);
         Assert.Equal(testUser.Phone, dbUser.Phone);
         Assert.Equal(testUser.Email, dbUser.Email);
      }

      [Theory]
      [MemberData(nameof(UserData))]
      public async Task UserAdapter_Get(User testUser)
      {
         var existingUser = await _repositoryAdapter.GetAsync(testUser);

         Assert.NotNull(existingUser);
         Assert.Equal(testUser.Id, existingUser.Id);
         Assert.Equal(testUser.Name, existingUser.Name);
         Assert.Equal(testUser.Phone, existingUser.Phone);
         Assert.Equal(testUser.Email, existingUser.Email);
      }

      [Theory]
      [MemberData(nameof(UserData))]
      public async Task UserAdapter_Update(User testUser)
      {
         var existingUser = await _repositoryAdapter.GetAsync(testUser);

         Assert.NotNull(existingUser);

         existingUser.Name = "New Name";
         existingUser.Email = "newtest@email.com";
         existingUser.Phone = "+526332014589";
         existingUser.UniqueKey = Guid.NewGuid();

         var updatedUser = await _repositoryAdapter.UpdateAsync(existingUser);

         Assert.NotNull(updatedUser);
         Assert.Equal(existingUser.Name, updatedUser.Name);
         Assert.Equal(existingUser.Email, updatedUser.Email);
         Assert.Equal(existingUser.Phone, updatedUser.Phone);
         Assert.Equal(existingUser.UniqueKey, updatedUser.UniqueKey);
      }

      [Theory]
      [MemberData(nameof(UserData))]
      public async Task UserAdapter_Delete(User testUser)
      {
         var existingUser = await _repositoryAdapter.GetAsync(testUser);

         Assert.NotNull(existingUser);

         var softDeleteUser = await _repositoryAdapter.DeleteAsync(existingUser);

         Assert.NotNull(softDeleteUser);
         Assert.Equal(existingUser.Id, softDeleteUser.Id);

         var noUser = await _repositoryAdapter.GetAsync(existingUser);

         Assert.Null(noUser);
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