using System;
using System.Net.Mime;
using Application.UseCases.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   internal class ExpenseRepository : IRepository<ExpenseEntity>
   {
      public async Task<ExpenseEntity?> CreateAsync(ExpenseEntity entity)
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

      public async Task<ExpenseEntity?> RetrieveAsync(ExpenseEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               var dbEntity = await context.FindAsync<ExpenseEntity>(entity.Id);

               if (dbEntity != null)
               {
                  return dbEntity;
               }
            }
            else if (entity.UniqueKey != Guid.Empty)
            {
               return await context.Expenses.FirstOrDefaultAsync(x => x.UniqueKey == entity.UniqueKey);
            }
         }

         return null;
      }

      public async Task<ExpenseEntity?> UpdateAsync(ExpenseEntity entity)
      {
         using (var context = new CoreContext())
         {
            var update = context.Update(entity);
            await context.SaveChangesAsync();
            return update.Entity;
         }
      }

      public async Task<ExpenseEntity?> DeleteAsync(ExpenseEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               context.Expenses.Attach(entity);
               context.Expenses.Remove(entity);
               await context.SaveChangesAsync();

               return entity;
            }
         }

         return null;
      }
   }
}