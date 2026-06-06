using System;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure.SqlDatabase
{
    public class PasswordRepositoryTests
    {
        private static DbContextOptions<CoreContext> CreateInMemoryOptions(string databaseName)
            => new DbContextOptionsBuilder<CoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

        [Fact]
        public async Task GivenNewPassword_WhenCreateAsync_ThenReturnsSavedPassword()
        {
            var options = CreateInMemoryOptions(nameof(GivenNewPassword_WhenCreateAsync_ThenReturnsSavedPassword));
            var repository = new PasswordRepository(() => new CoreContext(options));

            var password = new Password
            {
                UserId = 1,
                Cipher = "ABCD",
                Encrypted = "1234"
            };

            var created = await repository.CreateAsync(password);

            Assert.NotNull(created);
            Assert.Equal(1, created!.UserId);
            Assert.Equal("ABCD", created.Cipher);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Passwords.FirstOrDefaultAsync(x => x.UserId == 1);
            Assert.NotNull(stored);
            Assert.Equal("ABCD", stored!.Cipher);
        }

        [Fact]
        public async Task GivenPasswordExists_WhenFindByUserIdAsync_ThenReturnsPassword()
        {
            var options = CreateInMemoryOptions(nameof(GivenPasswordExists_WhenFindByUserIdAsync_ThenReturnsPassword));

            await using (var context = new CoreContext(options))
            {
                context.Passwords.Add(new PasswordEntity
                {
                    Id = 1,
                    UserId = 42,
                    Cipher = "ABCD",
                    Encrypted = "1234",
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var repository = new PasswordRepository(() => new CoreContext(options));
            var found = await repository.FindByUserIdAsync(42);

            Assert.NotNull(found);
            Assert.Equal(42, found!.UserId);
            Assert.Equal("ABCD", found.Cipher);
        }

        [Fact]
        public async Task GivenExistingPassword_WhenUpdateAsync_ThenUpdatesCipher()
        {
            var options = CreateInMemoryOptions(nameof(GivenExistingPassword_WhenUpdateAsync_ThenUpdatesCipher));

            await using (var context = new CoreContext(options))
            {
                context.Passwords.Add(new PasswordEntity
                {
                    Id = 1,
                    UserId = 99,
                    Cipher = "OLD",
                    Encrypted = "XXXX",
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var repository = new PasswordRepository(() => new CoreContext(options));
            var updated = await repository.UpdateAsync(new Password
            {
                UserId = 99,
                Cipher = "NEW",
                Encrypted = "YYYY"
            });

            Assert.NotNull(updated);
            Assert.Equal("NEW", updated!.Cipher);

            await using var verifyContext = new CoreContext(options);
            var stored = await verifyContext.Passwords.FirstOrDefaultAsync(x => x.UserId == 99);
            Assert.NotNull(stored);
            Assert.Equal("NEW", stored!.Cipher);
        }
    }
}
