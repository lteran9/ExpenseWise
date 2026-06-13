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
        private readonly Func<CoreContext> _contextFactory;

        public ExpenseRepository()
            : this(() => new CoreContext())
        {
        }

        public ExpenseRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<Expense?> CreateAsync(Expense expense)
        {
            var dbEntity = _adapter.MapEntityToDatabase(expense);

            // Prevent EF Core from trying to persist related Group/User
            // entities when creating an expense row.
            dbEntity.Group = null;
            dbEntity.CreatedBy = null;

            using (var context = _contextFactory())
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
            using (var context = _contextFactory())
            {
                var dbEntity = _adapter.MapEntityToDatabase(expense);
                var update = context.Update(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(update.Entity);
            }
        }

        public async Task<Expense?> DeleteAsync(Expense expense)
        {
            using (var context = _contextFactory())
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

            // Prevent EF Core from trying to persist related User/Group/Expense
            // entities when creating a split row.
            dbEntity.User = null;
            dbEntity.Group = null;
            dbEntity.Expense = null;

            if (dbEntity.CreatedAt == DateTime.MinValue) dbEntity.CreatedAt = DateTime.Now;
            if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;

            using (var context = _contextFactory())
            {
                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return
                    new Split
                    {
                        Id = insert.Entity.Id,
                        Paid = insert.Entity.Paid,
                        PaidOn = insert.Entity.PaidOn,
                        User = new User { Id = insert.Entity.UserId },
                        Group = new Group { Id = insert.Entity.GroupId },
                        Expense = new Expense { Id = insert.Entity.ExpenseId }
                    };
            }
        }

        public async Task<Split?> RemoveSplitAsync(Split split)
        {
            using (var context = _contextFactory())
            {
                var dbEntity = _adapter.MapEntityToDatabase(split);
                dbEntity.User = null;
                dbEntity.Group = null;
                dbEntity.Expense = null;
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

        public async Task<List<Expense>?> GetGroupExpenses(int groupId)
        {
            if (groupId > 0)
            {
                using (var context = _contextFactory())
                {
                    var expenses = await context.Expenses.Where(x => x.GroupId == groupId).ToListAsync();
                    return expenses.Select(x => _adapter.MapDatabaseToEntity(x)).ToList();
                }
            }

            return null;
        }
    }
}
