using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();
        private readonly Func<CoreContext> _contextFactory;

        public PasswordRepository()
            : this(() => new CoreContext())
        {
        }

        public PasswordRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<Password?> FindByUserIdAsync(int userId)
        {
            using (var context = _contextFactory())
            {
                var dbEntity = await context.Passwords.FirstOrDefaultAsync(x => x.UserId == userId);
                if (dbEntity != null)
                {
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<Password?> CreateAsync(Password password)
        {
            var dbEntity = _adapter.MapEntityToDatabase(password);

            using (var context = _contextFactory())
            {
                // Default values
                if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;

                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<Password?> UpdateAsync(Password password)
        {
            var dbEntity = _adapter.MapEntityToDatabase(password);

            using (var context = _contextFactory())
            {
                var existing = await context.Passwords.FirstOrDefaultAsync(x => x.UserId == dbEntity.UserId);
                if (existing == null)
                {
                    return null;
                }

                existing.Cipher = dbEntity.Cipher;
                existing.Encrypted = dbEntity.Encrypted;
                existing.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(existing);
            }
        }
    }
}
