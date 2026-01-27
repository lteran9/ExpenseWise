using System;
using Core.Entities;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure.AutoMapper
{
    public class ExpenseMapTests
    {
        [Fact]
        public void EquivalenceMap()
        {
            var dbExpense1 = new ExpenseEntity();
            var dbExpense2 = new ExpenseEntity();

            Assert.Equivalent(dbExpense1, dbExpense2);

            var expense1 = new Expense();
            var expense2 = new Expense();

            Assert.Equivalent(expense1, expense2);
        }

        [Fact]
        public void AutoMapper_DatabaseToEntity()
        {
            var amount = 100.00M;
            var currency = "USD";
            var expenseName = "Drinks @ the Club";

            var dbExpense =
                new ExpenseEntity()
                {
                    Id = 1,
                    Currency = currency,
                    Amount = amount,
                    Settled = true,
                    Description = expenseName
                };

            var expense =
                new Expense()
                {
                    Id = 1,
                    Currency = currency,
                    Amount = amount,
                    Settled = true,
                    Description = expenseName
                };

            Assert.Equivalent(expense, DatabaseMapper.Instance.Map<Expense>(dbExpense));
        }

        [Fact]
        public void AutoMapper_EntityToDatabase()
        {
            var amount = 100.00M;
            var currency = "USD";
            var expenseName = "Drinks @ the Club";

            var expense =
                new Expense()
                {
                    Id = 1,
                    Currency = currency,
                    Amount = amount,
                    Settled = true,
                    Description = expenseName
                };

            var dbExpense =
                new ExpenseEntity()
                {
                    Id = 1,
                    Currency = currency,
                    Amount = amount,
                    Settled = true,
                    Description = expenseName
                };

            Assert.Equivalent(dbExpense, DatabaseMapper.Instance.Map<ExpenseEntity>(expense));
        }
    }
}
