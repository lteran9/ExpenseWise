using Core.Entities;

namespace Tests.Regression.Core
{
    public class GroupLedgerTests
    {
        private static readonly User Ana = new() { Id = 1, Name = "Ana", UniqueKey = Guid.NewGuid() };
        private static readonly User Ben = new() { Id = 2, Name = "Ben", UniqueKey = Guid.NewGuid() };
        private static readonly User Cy = new() { Id = 3, Name = "Cy", UniqueKey = Guid.NewGuid() };

        private static Expense DinnerPaidByCy(bool voided = false) => new()
        {
            Amount = 40m,
            Voided = voided,
            Payer = Cy,
            Shares =
            [
                new ExpenseShare { Participant = Ana, Amount = 20m },
                new ExpenseShare { Participant = Ben, Amount = 20m }
            ]
        };

        [Fact]
        public void GroupLedger_WhenExpenseRecorded_BalancesSumToZero()
        {
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], []);

            Assert.Equal(0m, ledger.Balances.Sum(balance => balance.NetBalance));
            Assert.Equal(40m, ledger.TotalSpent);
            Assert.Equal(40m, ledger.Outstanding);
            Assert.Equal(-20m, ledger.BalanceOf(Ana.UniqueKey));
            Assert.Equal(40m, ledger.BalanceOf(Cy.UniqueKey));
        }

        [Fact]
        public void GroupLedger_WhenExpenseVoided_ExcludesItFromBalancesAndTotals()
        {
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy(voided: true)], []);

            Assert.Equal(0m, ledger.TotalSpent);
            Assert.Equal(0m, ledger.Outstanding);
            Assert.All(ledger.Balances, balance => Assert.Equal(0m, balance.NetBalance));
        }

        [Fact]
        public void GroupLedger_WhenRepaymentRecorded_ReducesBalancesWithoutChangingTotalSpent()
        {
            var repayment = new Repayment { Amount = 15m, Sender = Ana, Recipient = Cy };

            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], [repayment]);

            Assert.Equal(40m, ledger.TotalSpent);
            Assert.Equal(-5m, ledger.BalanceOf(Ana.UniqueKey));
            Assert.Equal(25m, ledger.BalanceOf(Cy.UniqueKey));
            Assert.Equal(25m, ledger.Outstanding);
            Assert.Equal(0m, ledger.Balances.Sum(balance => balance.NetBalance));
        }

        [Fact]
        public void SuggestRepayments_WhenDebtsExist_SettlesEveryBalance()
        {
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], []);

            var suggestions = ledger.SuggestRepayments();

            Assert.Equal(2, suggestions.Count);
            Assert.All(suggestions, suggestion => Assert.Equal(Cy.UniqueKey, suggestion.To.UniqueKey));
            Assert.Equal(40m, suggestions.Sum(suggestion => suggestion.Amount));
        }

        [Fact]
        public void ValidateRepayment_WhenAmountExceedsSenderDebt_ReturnsReason()
        {
            var repayment = new Repayment { Amount = 15m, Sender = Ana, Recipient = Cy };
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], [repayment]);

            var reason = ledger.ValidateRepayment(Ana.UniqueKey, Cy.UniqueKey, 6m);

            Assert.NotNull(reason);
            Assert.Contains("exceeds the sender's current debt", reason);
        }

        [Fact]
        public void ValidateRepayment_WhenWithinDebtAndCredit_ReturnsNull()
        {
            var repayment = new Repayment { Amount = 15m, Sender = Ana, Recipient = Cy };
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], [repayment]);

            Assert.Null(ledger.ValidateRepayment(Ana.UniqueKey, Cy.UniqueKey, 5m));
        }

        [Fact]
        public void ValidateRepayment_WhenRecipientIsNotOwed_ReturnsReason()
        {
            var ledger = new GroupLedger([Ana, Ben, Cy], [DinnerPaidByCy()], []);

            var reason = ledger.ValidateRepayment(Ana.UniqueKey, Ben.UniqueKey, 5m);

            Assert.Equal("The recipient is not owed money in this group.", reason);
        }
    }
}
