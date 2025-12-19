using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
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
        public void AutoMapper_DatabaseToEntity()
        {
            var uniqueKey = Guid.NewGuid();

            var user =
                new User()
                {
                    Id = 1000,
                    Name = "Test Tester",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            var dbUser =
                new UserEntity()
                {
                    Id = 1000,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            Assert.Equivalent(user, DatabaseMapper.UserMapper.Map<User>(dbUser));
        }

        [Fact]
        public void AutoMapper_EntityToDatabase()
        {
            var uniqueKey = Guid.NewGuid();

            var user =
                new User()
                {
                    Id = 1000,
                    Name = "Test Tester",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            var dbUser =
                new UserEntity()
                {
                    Id = 1000,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            Assert.Equivalent(dbUser, DatabaseMapper.UserMapper.Map<UserEntity>(user));
        }

        [Fact]
        public void AutoMapper_EntityToDatabase_NoLastName()
        {
            var uniqueKey = Guid.NewGuid();

            var user =
                new User()
                {
                    Id = 1000,
                    Name = "Quetzalcoatl",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            var dbUser =
                new UserEntity()
                {
                    Id = 1000,
                    FirstName = "Quetzalcoatl",
                    LastName = "",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            Assert.Equivalent(dbUser, DatabaseMapper.UserMapper.Map<UserEntity>(user));
        }

        [Fact]
        public void AutoMapper_EntityToDatabase_EmptyName()
        {
            var uniqueKey = Guid.NewGuid();

            // Empty Name

            var user =
                new User()
                {
                    Id = 1000,
                    Name = "",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            var dbUser =
                new UserEntity()
                {
                    Id = 1000,
                    FirstName = "",
                    LastName = "",
                    Email = "sample@test.com",
                    Phone = "+1 602 333 4578",
                    UniqueKey = uniqueKey
                };

            Assert.Equivalent(dbUser, DatabaseMapper.UserMapper.Map<UserEntity>(user));
        }
    }
}
