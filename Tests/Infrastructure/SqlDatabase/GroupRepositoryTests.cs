using System;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure.SqlDatabase
{
    public class GroupRepositoryTests
    {
        private static DbContextOptions<CoreContext> CreateInMemoryOptions(string databaseName)
            => new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

        [Fact]
        public async Task GivenNewGroup_WhenCreateAsync_ThenReturnsSavedGroup()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewGroup_WhenCreateAsync_ThenReturnsSavedGroup));
            var repository = new GroupRepository(() => new CoreContext(options));
            var uniqueKey = Guid.NewGuid();

            var group = new Group
            {
                Name = "Trip Group",
                UniqueKey = uniqueKey,
                Owner = new User { Id = 0 },
                Members = new System.Collections.Generic.List<User>()
            };

            var created = await repository.CreateAsync(group);

            Assert.NotNull(created);
            Assert.Equal(uniqueKey, created!.UniqueKey);
            Assert.Equal("Trip Group", created.Name);
            Assert.True(created.Active);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Groups.FirstOrDefaultAsync(x => x.UniqueKey == uniqueKey);
            Assert.NotNull(stored);
            Assert.Equal("Trip Group", stored!.Name);
        }

        [Fact]
        public async Task GivenGroupWithOwner_WhenFindByUniqueKeyAsync_ThenReturnsGroupAndOwner()
        {
            var options = CreateInMemoryOptions(nameof(GivenGroupWithOwner_WhenFindByUniqueKeyAsync_ThenReturnsGroupAndOwner));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Groups.Add(new GroupEntity
                {
                    Id = 1,
                    Name = "Owner Group",
                    UniqueKey = uniqueKey,
                    OwnerId = 1,
                    Active = true,
                    Owner = new UserEntity
                    {
                        Id = 1,
                        FirstName = "Test",
                        LastName = "Tester",
                        Email = "sample@test.com",
                        Phone = "16023334578",
                        UniqueKey = Guid.NewGuid(),
                        Active = true
                    }
                });
                await context.SaveChangesAsync();
            }

            var repository = new GroupRepository(() => new CoreContext(options));
            var found = await repository.FindByUniqueKeyAsync(uniqueKey);

            Assert.NotNull(found);
            Assert.Equal("Owner Group", found!.Name);
            Assert.NotNull(found.Owner);
            Assert.Equal("Test Tester", found.Owner.Name);
        }

        [Fact]
        public async Task GivenNewMember_WhenAddMemberAsync_ThenReturnsSavedMembership()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewMember_WhenAddMemberAsync_ThenReturnsSavedMembership));
            var repository = new GroupRepository(() => new CoreContext(options));

            var member = new MemberOf
            {
                User = new User { Id = 1, Name = "Test User" },
                Group = new Group { Id = 2, Name = "Test Group" }
            };

            var added = await repository.AddMemberAsync(member);

            Assert.NotNull(added);
            Assert.Equal(1, added!.User.Id);
            Assert.Equal(2, added.Group.Id);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.MemberOf.FirstOrDefaultAsync(x => x.UserId == 1 && x.GroupId == 2);
            Assert.NotNull(stored);
        }

        [Fact]
        public async Task GivenExistingMember_WhenRemoveMemberAsync_ThenRemovesMembership()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingMember_WhenRemoveMemberAsync_ThenRemovesMembership));

            await using (var context = new CoreContext(options))
            {
                context.MemberOf.Add(new MemberOfEntity
                {
                    Id = 1,
                    UserId = 1,
                    GroupId = 2,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var repository = new GroupRepository(() => new CoreContext(options));
            var removed = await repository.RemoveMemberAsync(new MemberOf { Id = 1, User = new User { Id = 1 }, Group = new Group { Id = 2 } });

            Assert.NotNull(removed);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.MemberOf.FindAsync(1);
            Assert.Null(stored);
        }

        [Fact]
        public async Task GivenExistingGroup_WhenUpdateAsync_ThenUpdatesGroupWithoutDuplicateKeyViolation()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingGroup_WhenUpdateAsync_ThenUpdatesGroupWithoutDuplicateKeyViolation));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Groups.Add(new GroupEntity
                {
                    Id = 1,
                    Name = "Original Group",
                    UniqueKey = uniqueKey,
                    OwnerId = 1,
                    Active = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Owner = new UserEntity
                    {
                        Id = 1,
                        FirstName = "Test",
                        LastName = "Tester",
                        Email = "sample@test.com",
                        Phone = "16023334578",
                        UniqueKey = Guid.NewGuid(),
                        Active = true
                    }
                });
                await context.SaveChangesAsync();
            }

            var repository = new GroupRepository(() => new CoreContext(options));
            var found = await repository.FindByUniqueKeyAsync(uniqueKey);

            Assert.NotNull(found);
            found!.Name = "Updated Group";
            found.Active = false;

            var updated = await repository.UpdateAsync(found);

            Assert.NotNull(updated);
            Assert.Equal("Updated Group", updated!.Name);
            Assert.False(updated.Active);
            Assert.Equal(uniqueKey, updated.UniqueKey);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Groups.FirstOrDefaultAsync(x => x.UniqueKey == uniqueKey);
            Assert.NotNull(stored);
            Assert.Equal("Updated Group", stored!.Name);
            Assert.False(stored.Active);
        }
    }
}
