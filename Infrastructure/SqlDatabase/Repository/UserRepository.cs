using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class UserRepository : IUserRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();

        public async Task<User?> CreateAsync(User user)
        {
            var dbEntity = _adapter.MapEntityToDatabase(user);

            using (var context = new CoreContext())
            {
                // Default values
                if (dbEntity.CreatedAt == DateTime.MinValue) dbEntity.CreatedAt = DateTime.Now;
                if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;
                if (dbEntity.UniqueKey == Guid.Empty) dbEntity.UniqueKey = Guid.NewGuid();

                // Strip characters from phone number to improve searchability
                dbEntity.Phone = StripPhoneOfCharacters(dbEntity.Phone);

                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            using (var context = new CoreContext())
            {
                var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
                // Only return active users
                if (dbEntity?.Active == true)
                {
                    dbEntity.Phone = FormatPhoneNumber(dbEntity.Phone);
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<User?> FindByPhoneAsync(string phone)
        {
            using (var context = new CoreContext())
            {
                var strippedPhone = StripPhoneOfCharacters(phone);
                var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.Phone == strippedPhone);
                // Only return active users
                if (dbEntity?.Active == true)
                {
                    dbEntity.Phone = FormatPhoneNumber(dbEntity.Phone);
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<User?> FindByUniqueKey(Guid key)
        {
            using (var context = new CoreContext())
            {
                var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.UniqueKey == key);
                if (dbEntity?.Active == true)
                {
                    dbEntity.Phone = FormatPhoneNumber(dbEntity.Phone);
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<User?> UpdateAsync(User user)
        {
            var dbEntity = _adapter.MapEntityToDatabase(user);

            using (var context = new CoreContext())
            {
                // Strip characters from phone number to improve searchability
                dbEntity.Phone = StripPhoneOfCharacters(dbEntity.Phone);
                // Updated at
                dbEntity.UpdatedAt = DateTime.Now;

                var update = context.Update(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(update.Entity);
            }
        }

        public async Task<User?> DeleteAsync(User user)
        {
            var dbEntity = _adapter.MapEntityToDatabase(user);

            using (var context = new CoreContext())
            {
                if (dbEntity.Id > 0)
                {
                    // Soft delete
                    dbEntity.Active = false;
                    var update = context.Update(dbEntity);
                    await context.SaveChangesAsync();
                    return _adapter.MapDatabaseToEntity(update.Entity);
                }
            }

            return null;
        }

        private string StripPhoneOfCharacters(string phone)
        {
            return phone
               .Replace("(", "")
               .Replace(")", "")
               .Replace("-", "")
               .Replace(" ", "");
        }

        private string FormatPhoneNumber(string phone)
        {
            return string.Format("{0:(###) ###-####}", phone);
        }
    }
}
