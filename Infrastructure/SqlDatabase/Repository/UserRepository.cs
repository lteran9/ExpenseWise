using System;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
   public class UserRepository : ISqlDatabase<UserEntity>
   {
      public async Task<UserEntity?> CreateAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            var insert = context.Add(entity);
            await context.SaveChangesAsync();
            return insert.Entity;
         }
      }

      public async Task<UserEntity?> DeleteAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            var delete = context.Remove(entity);
            await context.SaveChangesAsync();
            return delete.Entity;
         }
      }

      public async Task<UserEntity?> GetAsync(UserEntity entity)
      {
         using (var context = new CoreContext())
         {
            return await context.FindAsync<UserEntity>(entity.Id);
         }
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
   }
}