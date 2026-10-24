using System;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure.SqlDatabase
{
    public class UserRepositoryTests
    {
        private static DbContextOptions<CoreContext> CreateInMemoryOptions(string databaseName)
            => new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

        [Fact]
        public async Task GivenNewUser_WhenCreateAsync_ThenSavesUserWithStrippedPhoneAndGeneratedUniqueKey()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewUser_WhenCreateAsync_ThenSavesUserWithStrippedPhoneAndGeneratedUniqueKey));
            var repository = new UserRepository(() => new CoreContext(options));

            var user = new User
            {
                Name = "Test Tester",
                Email = "sample@test.com",
                Phone = "(602) 333-4578",
                UniqueKey = Guid.Empty
            };

            var created = await repository.CreateAsync(user);

            Assert.NotNull(created);
            Assert.Equal("sample@test.com", created!.Email);
            Assert.Equal("6023334578", created.Phone);
            Assert.NotEqual(Guid.Empty, created.UniqueKey);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Users.FirstOrDefaultAsync(x => x.Email == "sample@test.com");
            Assert.NotNull(stored);
            Assert.Equal("6023334578", stored!.Phone);
            Assert.NotEqual(Guid.Empty, stored.UniqueKey);
        }

        [Fact]
        public async Task GivenActiveUserExists_WhenFindByEmailAsync_ThenReturnsUserWithFormattedPhone()
        {
            var options = CreateInMemoryOptions(nameof(GivenActiveUserExists_WhenFindByEmailAsync_ThenReturnsUserWithFormattedPhone));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "16023334578",
                    UniqueKey = uniqueKey,
                    Active = true
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var found = await repository.FindByEmailAsync("sample@test.com");

            Assert.NotNull(found);
            Assert.Equal("sample@test.com", found!.Email);
            Assert.Equal("Test Tester", found.Name);
            Assert.Equal("(602) 333-4578", found.Phone);
            Assert.Equal(uniqueKey, found.UniqueKey);
        }

        [Fact]
        public async Task GivenActiveUserExists_WhenFindByPhoneAsync_ThenReturnsUserWithFormattedPhone()
        {
            var options = CreateInMemoryOptions(nameof(GivenActiveUserExists_WhenFindByPhoneAsync_ThenReturnsUserWithFormattedPhone));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "16023334578",
                    UniqueKey = uniqueKey,
                    Active = true
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var found = await repository.FindByPhoneAsync("(602) 333-4578");

            Assert.NotNull(found);
            Assert.Equal("sample@test.com", found!.Email);
            Assert.Equal("Test Tester", found.Name);
            Assert.Equal("(602) 333-4578", found.Phone);
            Assert.Equal(uniqueKey, found.UniqueKey);
        }

        [Fact]
        public async Task GivenUserWithSharedPhoneSuffix_WhenFindByCanonicalPhoneAsync_ThenRequiresExactPhoneMatch()
        {
            var options = CreateInMemoryOptions(nameof(GivenUserWithSharedPhoneSuffix_WhenFindByCanonicalPhoneAsync_ThenRequiresExactPhoneMatch));

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "523125551234",
                    UniqueKey = Guid.NewGuid(),
                    Active = true
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var matchingUser = await repository.FindByCanonicalPhoneAsync("523125551234");
            var nonMatchingUser = await repository.FindByCanonicalPhoneAsync("13125551234");

            Assert.NotNull(matchingUser);
            Assert.Null(nonMatchingUser);
        }

        [Fact]
        public async Task GivenActiveUserExists_WhenDeleteAsync_ThenSoftDeletesUser()
        {
            var options = CreateInMemoryOptions(nameof(GivenActiveUserExists_WhenDeleteAsync_ThenSoftDeletesUser));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "16023334578",
                    UniqueKey = uniqueKey,
                    Active = true
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var deleted = await repository.DeleteAsync(new User { Id = 1 });

            Assert.NotNull(deleted);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Users.FirstOrDefaultAsync(x => x.Id == 1);
            Assert.NotNull(stored);
            Assert.False(stored!.Active);
        }

        [Fact]
        public async Task GivenExistingUser_WhenUpdateAsync_ThenUpdatesUserWithoutDuplicateKeyViolation()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingUser_WhenUpdateAsync_ThenUpdatesUserWithoutDuplicateKeyViolation));
            var uniqueKey = Guid.NewGuid();

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "16023334578",
                    UniqueKey = uniqueKey,
                    Active = true
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var found = await repository.FindByUniqueKeyAsync(uniqueKey);

            Assert.NotNull(found);
            found!.Name = "Updated Name";
            found.Email = "updated@test.com";
            found.Phone = "16029998765";

            var updated = await repository.UpdateAsync(found);

            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.Equal("updated@test.com", updated.Email);
            Assert.Equal("16029998765", updated.Phone);
            Assert.Equal(uniqueKey, updated.UniqueKey);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Users.FirstOrDefaultAsync(x => x.UniqueKey == uniqueKey);
            Assert.NotNull(stored);
            Assert.Equal("Updated Name", stored!.FirstName + " " + stored.LastName);
            Assert.Equal("updated@test.com", stored.Email);
            Assert.Equal("16029998765", stored.Phone);
        }

        [Fact]
        public async Task GivenExistingUser_WhenUpdateAsync_ThenPreservesCreatedAt()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingUser_WhenUpdateAsync_ThenPreservesCreatedAt));
            var uniqueKey = Guid.NewGuid();
            var createdAt = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

            await using (var context = new CoreContext(options))
            {
                context.Users.Add(new UserEntity
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Tester",
                    Email = "sample@test.com",
                    Phone = "16023334578",
                    UniqueKey = uniqueKey,
                    Active = true,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                });
                await context.SaveChangesAsync();
            }

            var repository = new UserRepository(() => new CoreContext(options));
            var found = await repository.FindByUniqueKeyAsync(uniqueKey);
            found!.Email = "updated@test.com";

            await repository.UpdateAsync(found);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Users.SingleAsync(x => x.UniqueKey == uniqueKey);
            Assert.Equal(createdAt, stored.CreatedAt);
            Assert.Equal("updated@test.com", stored.Email);
        }
    }
}
