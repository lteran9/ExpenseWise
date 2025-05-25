using System;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
   internal class MemberOfRepository : IRepository<MemberOfEntity>
   {
      public async Task<MemberOfEntity?> CreateAsync(MemberOfEntity entity)
      {
         using (var context = new CoreContext())
         {
            // Default values
            if (entity.CreatedAt == DateTime.MinValue) entity.CreatedAt = DateTime.Now;
            if (entity.UpdatedAt == DateTime.MinValue) entity.UpdatedAt = DateTime.Now;

            entity.Group = null;
            entity.User = null;

            var insert = context.Add(entity);
            await context.SaveChangesAsync();
            return insert.Entity;
         }
      }

      public async Task<MemberOfEntity?> RetrieveAsync(MemberOfEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               return await context.FindAsync<MemberOfEntity>(entity.Id);
            }
         }

         return null;
      }

      public async Task<MemberOfEntity?> UpdateAsync(MemberOfEntity entity)
      {
         using (var context = new CoreContext())
         {
            var update = context.Update(entity);
            await context.SaveChangesAsync();
            return update.Entity;
         }
      }

      public async Task<MemberOfEntity?> DeleteAsync(MemberOfEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               context.MemberOf.Attach(entity);
               context.MemberOf.Remove(entity);
               await context.SaveChangesAsync();

               return entity;
            }
         }

         return null;
      }
   }
}