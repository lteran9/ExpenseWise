using System;
using Application.UseCases.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   internal class UserRepository : IRepository<UserEntity>
   {
      public async Task<UserEntity?> CreateAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            // Default values
            if (entity.CreatedAt == DateTime.MinValue) entity.CreatedAt = DateTime.Now;
            if (entity.UpdatedAt == DateTime.MinValue) entity.UpdatedAt = DateTime.Now;
            if (entity.UniqueKey == Guid.Empty) entity.UniqueKey = Guid.NewGuid();

            var insert = context.Add(entity);
            await context.SaveChangesAsync();
            return insert.Entity;
         }
      }

      public async Task<UserEntity?> RetrieveAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               var dbEntity = await context.FindAsync<UserEntity>(entity.Id);
               // Only return active users
               if (dbEntity?.Active == true)
               {
                  return dbEntity;
               }
            }
            else if (entity.UniqueKey != Guid.Empty)
            {
               var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.UniqueKey == entity.UniqueKey);
               // Only return active users
               if (dbEntity?.Active == true)
               {
                  return dbEntity;
               }
            }
            else if (!string.IsNullOrEmpty(entity.Email))
            {
               var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.Email == entity.Email);
               // Only return active users
               if (dbEntity?.Active == true)
               {
                  return dbEntity;
               }
            }
            else if (!string.IsNullOrEmpty(entity.Phone))
            {
               var dbEntity = await context.Users.FirstOrDefaultAsync(x => x.Phone == entity.Phone);
               if (dbEntity?.Active == true)
               {
                  return dbEntity;
               }
            }
         }

         return null;
      }

      public async Task<UserEntity?> UpdateAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            var update = context.Update(entity);
            await context.SaveChangesAsync();
            return update.Entity;
         }
      }

      /* Hard deletes involve deleting foreign key relationships as well. */

      public async Task<UserEntity?> DeleteAsync(UserEntity entity)
      {
         // Soft delete
         entity.Active = false;
         return await UpdateAsync(entity);
      }
   }
}