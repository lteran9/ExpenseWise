using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
{
    public class MemberOfMapTests
    {
        public static IEnumerable<object[]> DbEntityData =>
           new List<object[]>()
           {
                new object[] {
                    new UserEntity() { Id = 1000, FirstName = "Test", LastName = "Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() },
                    new GroupEntity() { Id = 1, OwnerId = 1000, Active = true, Name = "Last Vegas Trip",  UniqueKey = Guid.NewGuid() }
                }
           };

        public static IEnumerable<object[]> CoreEntityData =>
           new List<object[]>()
           {
                new object[] {
                    new User() { Id = 1000, Name = "Test Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() },
                    new Group() { Id = 1, Name = "Last Vegas Trip", UniqueKey = Guid.NewGuid() }
                }
           };

        [Fact]
        public void EquivalenceMap()
        {
            var dbUser1 = new MemberOfEntity();
            var dbUser2 = new MemberOfEntity();

            Assert.Equivalent(dbUser1, dbUser2);

            var user1 = new MemberOf();
            var user2 = new MemberOf();

            Assert.Equivalent(user1, user2);
        }

        [Theory]
        [MemberData(nameof(DbEntityData))]
        public void AutoMapper_DatabaseToEntity(UserEntity user, GroupEntity group)
        {
            var membership =
                new MemberOf()
                {
                    Id = 1,
                    User = DatabaseMapper.Instance.Map<User>(user),
                    Group = DatabaseMapper.Instance.Map<Group>(group)
                };

            var dbMembership =
                new MemberOfEntity()
                {
                    Id = 1,
                    UserId = user.Id,
                    User = user,
                    GroupId = group.Id,
                    Group = group,
                    Active = true
                };

            Assert.Equivalent(membership, DatabaseMapper.Instance.Map<MemberOf>(dbMembership));
        }

        [Theory]
        [MemberData(nameof(CoreEntityData))]
        public void AutoMapper_EntityToDatabase(User user, Group group)
        {
            // Arrange
            group.Owner = user;
            group.Members.Add(user);

            var membership =
                new MemberOf()
                {
                    Id = 1000,
                    User = user,
                    Group = group
                };

            var dbMembership =
                new MemberOfEntity()
                {
                    Id = 1000,
                    UserId = user.Id,
                    User = DatabaseMapper.Instance.Map<UserEntity>(user),
                    GroupId = group.Id,
                    Group = DatabaseMapper.Instance.Map<GroupEntity>(group),
                    Active = true
                };

            // Assert
            Assert.Equivalent(dbMembership, DatabaseMapper.Instance.Map<MemberOfEntity>(membership));
        }
    }
}
