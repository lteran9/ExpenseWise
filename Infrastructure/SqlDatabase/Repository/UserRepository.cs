using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class UserRepository : IUserRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();
        private readonly Func<CoreContext> _contextFactory;

        public UserRepository()
            : this(() => new CoreContext())
        {
        }

        public UserRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<User?> CreateAsync(User user)
        {
            var dbEntity = _adapter.MapEntityToDatabase(user);

            using (var context = _contextFactory())
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
            using (var context = _contextFactory())
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
            using (var context = _contextFactory())
            {
                var strippedPhone = NormalizeSearchPhone(phone);
                var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.Phone == strippedPhone || x.Phone.EndsWith(strippedPhone));
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
            using (var context = _contextFactory())
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

            using (var context = _contextFactory())
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

            using (var context = _contextFactory())
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
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        private string NormalizeSearchPhone(string phone)
        {
            var digits = StripPhoneOfCharacters(phone);
            return digits.Length > 10 ? digits[^10..] : digits;
        }

        private string FormatPhoneNumber(string phone)
        {
            var digits = StripPhoneOfCharacters(phone);
            if (digits.Length == 11 && digits.StartsWith("1"))
            {
                digits = digits[1..];
            }

            if (digits.Length != 10)
            {
                return phone;
            }

            return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
        }
    }
}
