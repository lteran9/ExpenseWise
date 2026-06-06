using System;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();

        public async Task<Expense?> CreateAsync(Expense expense)
        {
            var dbEntity = _adapter.MapEntityToDatabase(expense);

            using (var context = new CoreContext())
            {
                // Default values
                if (dbEntity.CreatedAt == DateTime.MinValue) dbEntity.CreatedAt = DateTime.Now;
                if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;
                if (dbEntity.UniqueKey == Guid.Empty) dbEntity.UniqueKey = Guid.NewGuid();

                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<Expense?> UpdateAsync(Expense expense)
        {
            using (var context = new CoreContext())
            {
                var dbEntity = _adapter.MapEntityToDatabase(expense);
                var update = context.Update(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(update.Entity);
            }
        }

        public async Task<Expense?> DeleteAsync(Expense expense)
        {
            using (var context = new CoreContext())
            {
                var dbEntity = _adapter.MapEntityToDatabase(expense);
                if (dbEntity.Id > 0)
                {
                    context.Expenses.Attach(dbEntity);
                    context.Expenses.Remove(dbEntity);
                    await context.SaveChangesAsync();

                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<Split?> AddSplitAsync(Split split)
        {
            var dbEntity = _adapter.MapEntityToDatabase(split);

            using (var context = new CoreContext())
            {
                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<Split?> RemoveSplitAsync(Split split)
        {
            using (var context = new CoreContext())
            {
                var dbEntity = _adapter.MapEntityToDatabase(split);
                if (dbEntity.Id > 0)
                {
                    context.Splits.Attach(dbEntity);
                    context.Splits.Remove(dbEntity);
                    await context.SaveChangesAsync();

                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }
    }
}
