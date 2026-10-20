using System;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;
using MySql.Data.MySqlClient;

namespace Infrastructure.SqlDatabase
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();
        private readonly Func<CoreContext> _contextFactory;

        public ExpenseRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<(Expense Expense, bool Created)> CreateWithSharesAsync(Expense expense, CancellationToken cancellationToken)
        {
            // Fast path for idempotent retries that never waits on the group lock. The lookup inside the lock is authoritative.
            using (var probe = _contextFactory())
            {
                var previous = await LoadByUniqueKeyAsync(probe, expense.UniqueKey, cancellationToken);
                if (previous != null)
                {
                    return (MapExpense(previous), false);
                }
            }

            using var context = _contextFactory();

            await using var transaction = context.Database.IsRelational()
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : null;

            await GroupWriteLock.AcquireAsync(context, expense.BelongsTo!.Id, cancellationToken);
            var existing = await LoadByUniqueKeyAsync(context, expense.UniqueKey, cancellationToken);
            if (existing != null)
            {
                return (MapExpense(existing), false);
            }

            var participantIds = expense.Shares.Select(share => share.Participant.Id)
                .Append(expense.Payer!.Id)
                .Distinct()
                .ToList();
            var activeCount = await context.MemberOf.CountAsync(
                membership => membership.GroupId == expense.BelongsTo!.Id
                    && membership.Active
                    && participantIds.Contains(membership.UserId),
                cancellationToken);
            if (activeCount != participantIds.Count)
            {
                throw new GroupMembershipConflictException("The payer and every participant must still be members of the group.");
            }

            try
            {
                var entity = _adapter.MapEntityToDatabase(expense);
                entity.Group = null;
                entity.CreatedBy = null;
                entity.Payer = null;
                entity.Shares = [];
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = entity.CreatedAt;

                context.Expenses.Add(entity);
                await context.SaveChangesAsync(cancellationToken);

                var shares = expense.Shares.Select(share => new ExpenseShareEntity
                {
                    ExpenseId = entity.Id,
                    UserId = share.Participant.Id,
                    Amount = share.Amount,
                    CreatedAt = entity.CreatedAt
                });
                await context.ExpenseShares.AddRangeAsync(shares, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                entity.PayerUserId = expense.Payer!.Id;
                entity.Payer = new UserEntity { Id = expense.Payer.Id, UniqueKey = expense.Payer.UniqueKey };
                entity.CreatedBy = new UserEntity { Id = expense.CreatedBy!.Id, UniqueKey = expense.CreatedBy.UniqueKey };
                entity.Shares = await context.ExpenseShares
                    .Where(share => share.ExpenseId == entity.Id)
                    .Include(share => share.Participant)
                    .ToListAsync(cancellationToken);
                return (MapExpense(entity), true);
            }
            catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    await transaction.DisposeAsync();
                }

                using var retryContext = _contextFactory();
                var concurrent = await LoadByUniqueKeyAsync(retryContext, expense.UniqueKey, cancellationToken);
                if (concurrent == null)
                {
                    throw;
                }

                return (MapExpense(concurrent), false);
            }
        }

        public async Task<Expense?> FindByUniqueKeyAsync(Guid uniqueKey, CancellationToken cancellationToken)
        {
            using var context = _contextFactory();
            var entity = await LoadByUniqueKeyAsync(context, uniqueKey, cancellationToken);
            return entity == null ? null : MapExpense(entity);
        }

        public async Task<VoidExpenseOutcome> VoidAsync(Guid uniqueKey, int groupId, CancellationToken cancellationToken)
        {
            using var context = _contextFactory();

            await using var transaction = context.Database.IsRelational()
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : null;

            await GroupWriteLock.AcquireAsync(context, groupId, cancellationToken);
            var entity = await context.Expenses
                .FirstOrDefaultAsync(expense => expense.UniqueKey == uniqueKey && expense.GroupId == groupId, cancellationToken);
            if (entity == null)
            {
                return new VoidExpenseOutcome(null, false);
            }

            if (entity.Voided)
            {
                return new VoidExpenseOutcome(MapExpense(entity), false);
            }

            if (await context.Repayments.AnyAsync(repayment => repayment.GroupId == groupId, cancellationToken))
            {
                return new VoidExpenseOutcome(null, true);
            }

            entity.Voided = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new VoidExpenseOutcome(MapExpense(entity), false);
        }

        public async Task<List<Expense>?> GetGroupExpenses(int groupId)
        {
            if (groupId > 0)
            {
                using (var context = _contextFactory())
                {
                    var expenses = await context.Expenses
                        .Where(expense => expense.GroupId == groupId)
                        .Include(expense => expense.Payer)
                        .Include(expense => expense.CreatedBy)
                        .Include(expense => expense.Shares)
                            .ThenInclude(share => share.Participant)
                        .ToListAsync();
                    return expenses.Select(MapExpense).ToList();
                }
            }

            return null;
        }

        private async Task<ExpenseEntity?> LoadByUniqueKeyAsync(
            CoreContext context,
            Guid key,
            CancellationToken cancellationToken)
        {
            return await context.Expenses
                .Include(expense => expense.Group)
                .Include(expense => expense.Payer)
                .Include(expense => expense.CreatedBy)
                .Include(expense => expense.Shares)
                    .ThenInclude(share => share.Participant)
                .FirstOrDefaultAsync(expense => expense.UniqueKey == key, cancellationToken);
        }

        private Expense MapExpense(ExpenseEntity entity) => _adapter.MapExpenseWithShares(entity);
    }
}
