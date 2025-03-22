using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class UserTests
   {
      private readonly ISqlDatabase<UserEntity> _repository = new UserRepository();

      [Fact]
      public async Task Create()
      {
         var user =
            new UserEntity()
            {
               FirstName = "Test",
               LastName = "Tester",
               Email = "test@example.com",
               Phone = "6023334578",
               Password = "emptypassword",
               CreatedAt = DateTime.Now,
               UpdatedAt = DateTime.Now
            };

         var dbUser = await _repository.CreateAsync(user);

         Assert.NotNull(dbUser);
         Assert.True(dbUser.Id > 0);
         Assert.True(dbUser.Active);
      }

      [Fact]
      public async Task Update()
      {

         var dbUser = await _repository.GetAsync(new UserEntity() { Id = 1 });

         Assert.NotNull(dbUser);

         // Change name
         dbUser.FirstName = "Newer Test";
         dbUser.UpdatedAt = DateTime.Now;
         var result = await _repository.UpdateAsync(dbUser);

         Assert.NotNull(result);
         Assert.Equal(dbUser.FirstName, result.FirstName);
         Assert.Equal(dbUser.UpdatedAt, result.UpdatedAt);
      }

      [Fact]
      public async Task HardDelete()
      {
         var dbUser = await _repository.GetAsync(new UserEntity() { Id = 1 });

         Assert.NotNull(dbUser);

         var result = await _repository.DeleteAsync(dbUser);

         Assert.NotNull(result);

         var missingUser = await _repository.GetAsync(result);

         Assert.Null(missingUser);
      }

      [Fact]
      public async Task SoftDelete()
      {
         var dbUser = await _repository.GetAsync(new UserEntity() { Id = 1 });

         Assert.NotNull(dbUser);

         // Set inactive
         dbUser.Active = false;
         dbUser.UpdatedAt = DateTime.Now;
         var result = await _repository.UpdateAsync(dbUser);

         Assert.NotNull(result);
         Assert.Equal(dbUser.Active, result.Active);
         Assert.Equal(dbUser.UpdatedAt, result.UpdatedAt);
      }
   }
}