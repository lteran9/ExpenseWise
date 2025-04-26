using System;
using System.Net.Mime;
using Application.UseCases.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   internal class PasswordRepository : IRepository<PasswordEntity>
   {
      public async Task<PasswordEntity?> CreateAsync(PasswordEntity entity)
      {
         using (var context = new CoreContext())
         {
            // Default values
            if (entity.UpdatedAt == DateTime.MinValue) entity.UpdatedAt = DateTime.Now;

            var insert = context.Add(entity);
            await context.SaveChangesAsync();
            return insert.Entity;
         }
      }

      public async Task<PasswordEntity?> RetrieveAsync(PasswordEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               return await context.FindAsync<PasswordEntity>(entity.Id);
            }
            else if (entity.UserId > 0)
            {
               return await context.Passwords.FirstOrDefaultAsync(x => x.UserId == entity.UserId);
            }
         }

         return null;
      }

      public async Task<PasswordEntity?> UpdateAsync(PasswordEntity entity)
      {
         using (var context = new CoreContext())
         {
            var update = context.Update(entity);
            await context.SaveChangesAsync();
            return update.Entity;
         }
      }

      public async Task<PasswordEntity?> DeleteAsync(PasswordEntity entity)
      {
         using (var context = new CoreContext())
         {
            if (entity.Id > 0)
            {
               context.Passwords.Attach(entity);
               context.Passwords.Remove(entity);
               await context.SaveChangesAsync();

               return entity;
            }
         }

         return null;
      }
   }
}