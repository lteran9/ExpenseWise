using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;
using MySql.Data.MySqlClient;

namespace Infrastructure.SqlDatabase
{
    public class RepaymentRepository : IRepaymentRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();
        private readonly Func<CoreContext> _contextFactory;

        public RepaymentRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<List<Repayment>> GetGroupRepaymentsAsync(int groupId, CancellationToken cancellationToken)
        {
            if (groupId <= 0)
            {
                return [];
            }

            using var context = _contextFactory();
            var entities = await context.Repayments
                .Where(repayment => repayment.GroupId == groupId)
                .Include(repayment => repayment.Group)
                .Include(repayment => repayment.Sender)
                .Include(repayment => repayment.Recipient)
                .Include(repayment => repayment.RecordedBy)
                .OrderBy(repayment => repayment.OccurredAt)
                .ThenBy(repayment => repayment.Id)
                .ToListAsync(cancellationToken);
            return entities.Select(_adapter.MapDatabaseToEntity).ToList();
        }

        public async Task<RecordRepaymentOutcome> RecordAsync(
            Repayment repayment,
            Func<GroupLedger, string?> validateAgainstLedger,
            CancellationToken cancellationToken)
        {
            var groupId = repayment.BelongsTo!.Id;

            using var context = _contextFactory();
            await using var transaction = context.Database.IsRelational()
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : null;

            try
            {
                await GroupWriteLock.AcquireAsync(context, groupId, cancellationToken);

                var existing = await LoadByUniqueKeyAsync(context, repayment.UniqueKey, cancellationToken);
                if (existing != null)
                {
                    return new RecordRepaymentOutcome(_adapter.MapDatabaseToEntity(existing), false, null);
                }

                var ledger = await BuildLedgerAsync(context, groupId, cancellationToken);
                var rejection = validateAgainstLedger(ledger);
                if (rejection != null)
                {
                    return new RecordRepaymentOutcome(null, false, rejection);
                }

                var entity = _adapter.MapEntityToDatabase(repayment);
                entity.CreatedAt = DateTime.UtcNow;
                context.Repayments.Add(entity);
                await context.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                var stored = await LoadByUniqueKeyAsync(context, repayment.UniqueKey, cancellationToken);
                return new RecordRepaymentOutcome(_adapter.MapDatabaseToEntity(stored!), true, null);
            }
            catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                using var retryContext = _contextFactory();
                var concurrent = await LoadByUniqueKeyAsync(retryContext, repayment.UniqueKey, cancellationToken);
                if (concurrent == null)
                {
                    throw;
                }

                return new RecordRepaymentOutcome(_adapter.MapDatabaseToEntity(concurrent), false, null);
            }
        }

        private async Task<GroupLedger> BuildLedgerAsync(CoreContext context, int groupId, CancellationToken cancellationToken)
        {
            var members = await context.MemberOf
                .Where(membership => membership.GroupId == groupId && membership.Active && membership.User != null)
                .Include(membership => membership.User)
                .ToListAsync(cancellationToken);

            var expenses = await context.Expenses
                .Where(expense => expense.GroupId == groupId)
                .Include(expense => expense.Payer)
                .Include(expense => expense.Shares)
                    .ThenInclude(share => share.Participant)
                .ToListAsync(cancellationToken);

            var repayments = await context.Repayments
                .Where(repayment => repayment.GroupId == groupId)
                .Include(repayment => repayment.Sender)
                .Include(repayment => repayment.Recipient)
                .ToListAsync(cancellationToken);

            return new GroupLedger(
                members.Select(membership => _adapter.MapDatabaseToEntity(membership.User!)),
                expenses.Select(_adapter.MapExpenseWithShares),
                repayments.Select(_adapter.MapDatabaseToEntity));
        }

        private static Task<RepaymentEntity?> LoadByUniqueKeyAsync(
            CoreContext context,
            Guid uniqueKey,
            CancellationToken cancellationToken)
        {
            return context.Repayments
                .Include(repayment => repayment.Group)
                .Include(repayment => repayment.Sender)
                .Include(repayment => repayment.Recipient)
                .Include(repayment => repayment.RecordedBy)
                .FirstOrDefaultAsync(repayment => repayment.UniqueKey == uniqueKey, cancellationToken);
        }
    }
}
