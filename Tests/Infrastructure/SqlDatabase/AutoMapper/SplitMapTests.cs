using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
{
    public class SplitMapTests
    {
        public static IEnumerable<object[]> DbEntityData =>
           new List<object[]>()
           {
                new object[] {
                    new UserEntity() { Id = 1000, FirstName = "Test", LastName = "Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() },
                    new GroupEntity() { Id = 1, OwnerId = 1000, Active = true, Name = "Last Vegas Trip",  UniqueKey = Guid.NewGuid() },
                    new ExpenseEntity() { Id = 1, Amount = 100.00M, Currency = "USD", Description = "Drinks @ the Club", UniqueKey = Guid.NewGuid() }
                }
           };

        public static IEnumerable<object[]> CoreEntityData =>
           new List<object[]>()
           {
                new object[] {
                    new User() { Id = 1000, Name = "Test Tester", Email = "test@example.com", Phone = "+16023334578", UniqueKey = Guid.NewGuid() },
                    new Group() { Id = 1, Active = true, Name = "Last Vegas Trip",  UniqueKey = Guid.NewGuid() },
                    new Expense() { Id = 1, Amount = 100.00M, Currency = "USD", Description = "Drinks @ the Club" }
                }
           };

        [Fact]
        public void EquivalenceMap()
        {
            var dbSplit1 = new SplitEntity();
            var dbSplit2 = new SplitEntity();

            Assert.Equivalent(dbSplit1, dbSplit2);

            var split1 = new Split();
            var split2 = new Split();

            Assert.Equivalent(split1, split2);
        }

        [Theory]
        [MemberData(nameof(DbEntityData))]
        public void AutoMapper_DatabaseToEntity(UserEntity user, GroupEntity group, ExpenseEntity expense)
        {
            var dbSplit =
                new SplitEntity()
                {
                    Id = 1,
                    Paid = false,
                    PaidOn = DateTime.MinValue,
                    UserId = user.Id,
                    User = user,
                    GroupId = group.Id,
                    Group = group,
                    ExpenseId = expense.Id,
                    Expense = expense
                };

            var split =
                new Split()
                {
                    Id = 1,
                    Paid = false,
                    PaidOn = DateTime.MinValue,
                    User = DatabaseMapper.Instance.Map<User>(user),
                    Group = DatabaseMapper.Instance.Map<Group>(group),
                    Expense = DatabaseMapper.Instance.Map<Expense>(expense)
                };

            Assert.Equivalent(split, DatabaseMapper.Instance.Map<Split>(dbSplit));
        }

        [Theory]
        [MemberData(nameof(CoreEntityData))]
        public void AutoMapper_EntityToDatabase(User user, Group group, Expense expense)
        {
            var split =
                new Split()
                {
                    Id = 1,
                    Paid = false,
                    PaidOn = DateTime.MinValue,
                    User = user,
                    Group = group,
                    Expense = expense
                };

            var dbSplit =
                new SplitEntity()
                {
                    Id = 1,
                    Paid = false,
                    PaidOn = DateTime.MinValue,
                    UserId = user.Id,
                    User = DatabaseMapper.Instance.Map<UserEntity>(user),
                    GroupId = group.Id,
                    Group = DatabaseMapper.Instance.Map<GroupEntity>(group),
                    ExpenseId = expense.Id,
                    Expense = DatabaseMapper.Instance.Map<ExpenseEntity>(expense)
                };

            var mappedEntity = DatabaseMapper.Instance.Map<SplitEntity>(split);

            Assert.Equivalent(dbSplit, mappedEntity);
        }
    }
}
