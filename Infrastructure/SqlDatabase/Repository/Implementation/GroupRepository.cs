using System;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
   internal class GroupRepository : IRepository<GroupEntity>
   {
      public async Task<GroupEntity?> CreateAsync(GroupEntity entity)
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

      public async Task<GroupEntity?> RetrieveAsync(GroupEntity entity)
      {
         using (var context = new CoreContext())
         {
            var dbEntity = await context.FindAsync<GroupEntity>(entity.Id);
            // Only return active records for now
            if (dbEntity?.Active == true)
            {
               return dbEntity;
            }
         }

         return null;
      }

      public async Task<GroupEntity?> UpdateAsync(GroupEntity entity)
      {
         using (var context = new CoreContext())
         {
            var update = context.Update(entity);
            await context.SaveChangesAsync();
            return update.Entity;
         }
      }

      /* Hard deletes involve deleting foreign key relationships as well. */
      public async Task<GroupEntity?> DeleteAsync(GroupEntity entity)
      {
         // Soft delete
         entity.Active = false;

         return await UpdateAsync(entity);
      }
   }
}