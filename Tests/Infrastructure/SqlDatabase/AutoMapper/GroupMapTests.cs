using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
{
    public class GroupMapTests
    {
        public static IEnumerable<object[]> DbUserData =>
            new List<object[]>()
            {
                new object[] { new UserEntity() { Id = 1000, FirstName = "Test", LastName = "Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() } }
            };

        public static IEnumerable<object[]> CoreUserData =>
            new List<object[]>()
            {
                new object[] { new User() { Id = 1000, Name = "Test Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() } }
            };

        [Fact]
        public void EquivalenceMap()
        {
            var dbGroup1 = new GroupEntity();
            var dbGroup2 = new GroupEntity();

            Assert.Equivalent(dbGroup1, dbGroup2);

            var group1 = new Group();
            var group2 = new Group();

            Assert.Equivalent(group1, group2);
        }

        [Theory]
        [MemberData(nameof(DbUserData))]
        public void AutoMapper_DatabaseToEntity(UserEntity dbOwner)
        {
            var uniqueKey = Guid.NewGuid();

            var dbGroup =
                new GroupEntity()
                {
                    Id = 1000,
                    Name = "Las Vegas 2025 Trip",
                    UniqueKey = uniqueKey,
                    OwnerId = dbOwner.Id,
                    Owner = dbOwner,
                    Membership = new List<MemberOfEntity>() { new MemberOfEntity() { User = dbOwner } }
                };

            var group =
                new Group()
                {
                    Id = 1000,
                    Name = "Las Vegas 2025 Trip",
                    UniqueKey = uniqueKey,
                    Owner = DatabaseMapper.UserMapper.Map<User>(dbOwner),
                    Members = new List<User>() { DatabaseMapper.UserMapper.Map<User>(dbOwner) }
                };

            var mappedGroup = DatabaseMapper.GroupMapper.Map<Group>(dbGroup);

            Assert.Equivalent(group, mappedGroup);
        }

        [Theory]
        [MemberData(nameof(CoreUserData))]
        public void AutoMapper_EntityToDatabase(User owner)
        {
            var groupName = "Trip Gas Expense";
            var uniqueKey = Guid.NewGuid();

            var user =
                new Group()
                {
                    Id = 1000,
                    Name = groupName,
                    UniqueKey = uniqueKey,
                    Owner = owner,
                    Members = new List<User>() { owner }
                };

            var dbGroup =
                new GroupEntity()
                {
                    Id = 1000,
                    Name = groupName,
                    UniqueKey = uniqueKey,
                    OwnerId = owner.Id,
                    Owner = DatabaseMapper.UserMapper.Map<UserEntity>(owner)
                };

            Assert.Equivalent(dbGroup, DatabaseMapper.GroupMapper.Map<GroupEntity>(user));
        }
    }
}
