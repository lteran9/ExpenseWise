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
        Task<(Expense Expense, bool Created)> CreateWithSharesAsync(Expense expense, CancellationToken cancellationToken);
        Task<Expense?> FindByUniqueKeyAsync(Guid uniqueKey, CancellationToken cancellationToken);
        Task<Expense?> VoidAsync(Guid uniqueKey, int groupId, CancellationToken cancellationToken);
    }
}