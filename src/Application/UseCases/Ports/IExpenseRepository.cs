using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IExpenseRepository
    {
        Task<List<Expense>?> GetGroupExpenses(int groupId);
        /// <summary>
        /// Creates the expense and its shares, or returns the expense already stored under the same unique key.
        /// </summary>
        /// <exception cref="GroupMembershipConflictException">
        /// The payer or a participant is no longer an active group member at write time.
        /// </exception>
        Task<(Expense Expense, bool Created)> CreateWithSharesAsync(Expense expense, CancellationToken cancellationToken);
        Task<Expense?> FindByUniqueKeyAsync(Guid uniqueKey, CancellationToken cancellationToken);
        /// <summary>
        /// Voids the expense unless the group already has repayments. The check and the update run under the same group lock.
        /// </summary>
        Task<VoidExpenseOutcome> VoidAsync(Guid uniqueKey, int groupId, CancellationToken cancellationToken);
    }

    public sealed record VoidExpenseOutcome(Expense? Expense, bool BlockedByRepayments);
}