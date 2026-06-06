using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();

        public async Task<Password?> FindByUserIdAsync(int userId)
        {
            using (var context = new CoreContext())
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

            using (var context = new CoreContext())
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

            using (var context = new CoreContext())
            {
                var update = context.Update(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(update.Entity);
            }
        }
    }
}
