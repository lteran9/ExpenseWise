using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class UserMapTests
   {
      [Fact]
      public void EquivalenceMap()
      {
         var dbUser1 = new UserEntity();
         var dbUser2 = new UserEntity();

         Assert.Equivalent(dbUser1, dbUser2);

         var user1 = new User();
         var user2 = new User();

         Assert.Equivalent(user1, user2);
      }

      [Fact]
      public void BaseMap_DatabaseToEntity()
      {
         var uniqueKey = Guid.NewGuid();

         var user =
            new User()
            {
               Id = 1000,
               Name = "Luis Teran",
               Email = "sample@test.com",
               Phone = "+1 602 333 4578",
               UniqueKey = uniqueKey
            };

         var dbUser =
            new UserEntity()
            {
               Id = 1000,
               FirstName = "Luis",
               LastName = "Teran",
               Email = "sample@test.com",
               Phone = "+1 602 333 4578",
               UniqueKey = uniqueKey
            };

         Assert.Equivalent(user, DatabaseMapper.UserMapper.Map<User>(dbUser));
      }

      [Fact]
      public void BaseMap_EntityToDatabase()
      {
         var uniqueKey = Guid.NewGuid();

         var user =
            new User()
            {
               Id = 1000,
               Name = "Luis Teran",
               Email = "sample@test.com",
               Phone = "+1 602 333 4578",
               UniqueKey = uniqueKey
            };

         var dbUser =
            new UserEntity()
            {
               Id = 1000,
               FirstName = "Luis",
               LastName = "Teran",
               Email = "sample@test.com",
               Phone = "+1 602 333 4578",
               UniqueKey = uniqueKey
            };

         Assert.Equivalent(dbUser, DatabaseMapper.UserMapper.Map<UserEntity>(user));
      }
   }
}