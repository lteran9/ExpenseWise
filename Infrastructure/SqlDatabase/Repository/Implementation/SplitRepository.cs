using System;

namespace Infrastructure.SqlDatabase
{
    internal class SplitRepository : IRepository<SplitEntity>
    {
        public async Task<SplitEntity?> CreateAsync(SplitEntity entity)
        {
            using (var context = new CoreContext())
            {
                // Default values
                if (entity.CreatedAt == DateTime.MinValue) entity.CreatedAt = DateTime.Now;
                if (entity.UpdatedAt == DateTime.MinValue) entity.UpdatedAt = DateTime.Now;

                // Relationships should already exist by the time we are creating a Split record
                entity.User = null;
                entity.Expense = null;
                entity.Group = null;

                var insert = context.Add(entity);
                await context.SaveChangesAsync();
                return insert.Entity;
            }
        }

        public async Task<SplitEntity?> RetrieveAsync(SplitEntity entity)
        {
            using (var context = new CoreContext())
            {
                if (entity.Id > 0)
                {
                    return await context.FindAsync<SplitEntity>(entity.Id);
                }
            }

            return null;
        }

        public async Task<SplitEntity?> UpdateAsync(SplitEntity entity)
        {
            using (var context = new CoreContext())
            {
                var update = context.Update(entity);
                await context.SaveChangesAsync();
                return update.Entity;
            }
        }

        public async Task<SplitEntity?> DeleteAsync(SplitEntity entity)
        {
            using (var context = new CoreContext())
            {
                if (entity.Id > 0)
                {
                    context.Splits.Attach(entity);
                    context.Splits.Remove(entity);
                    await context.SaveChangesAsync();

                    return entity;
                }
            }

            return null;
        }
    }
}
