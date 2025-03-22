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

      [Fact]
      public async Task UserAdapter_Create()
      {
         var user =
            new User()
            {
               Name = "Luis Teran",
               Phone = "+16023334578",
               Email = "test@email.com"
            };

         var dbUser = await _repositoryAdapter.CreateAsync(user);

         Assert.NotNull(dbUser);
         Assert.True(dbUser.Id > 0);
         Assert.Equal(user.Name, dbUser.Name);
         Assert.Equal(user.Phone, dbUser.Phone);
         Assert.Equal(user.Email, dbUser.Email);
      }

      [Fact]
      public async Task UserAdapter_Get()
      {
         var user =
            new User()
            {
               Id = 1
            };

         var existingUser = await _repositoryAdapter.GetAsync(user);

         Assert.NotNull(existingUser);
         Assert.True(existingUser.Id == user.Id);
         Assert.NotEmpty(existingUser.Name);
         Assert.NotEmpty(existingUser.Phone);
         Assert.NotEmpty(existingUser.Email);
      }

      [Fact]
      public async Task UserAdapter_Update()
      {
         var existingUser = await _repositoryAdapter.GetAsync(new User() { Id = 1 });

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

      [Fact]
      public async Task UserAdapter_Delete()
      {
         var existingUser = await _repositoryAdapter.GetAsync(new User() { Id = 1 });

         Assert.NotNull(existingUser);

         var softDeleteUser = await _repositoryAdapter.DeleteAsync(existingUser);

         Assert.NotNull(softDeleteUser);
         Assert.Equal(existingUser.Id, softDeleteUser.Id);

         var noUser = await _repositoryAdapter.GetAsync(new User() { Id = existingUser.Id });

         Assert.Null(noUser);
      }
   }
}