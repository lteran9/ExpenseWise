using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IExpenseRepository
    {
        Task<List<Expense>?> GetGroupExpenses(int groupId);
        Task<Expense?> CreateAsync(Expense expense);
        Task<Expense?> UpdateAsync(Expense expense);
        Task<Expense?> DeleteAsync(Expense expense);
        Task<Split?> AddSplitAsync(Split split);
        Task<Split?> RemoveSplitAsync(Split split);
    }
}