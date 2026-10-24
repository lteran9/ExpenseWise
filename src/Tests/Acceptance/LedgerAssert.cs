namespace Tests.Acceptance
{
    public static class LedgerAssert
    {
        public static void Balances(LedgerSnapshot ledger, params (TestUser Member, string Status)[] expected)
        {
            Assert.Equal(
                expected.ToDictionary(item => item.Member.Name, item => item.Status).OrderBy(pair => pair.Key),
                ledger.Balances.OrderBy(pair => pair.Key));
        }

        public static void Unchanged(LedgerSnapshot expected, LedgerSnapshot actual)
        {
            Assert.Equal(expected.TotalSpent, actual.TotalSpent);
            Assert.Equal(expected.Outstanding, actual.Outstanding);
            Assert.Equal(expected.Balances.OrderBy(pair => pair.Key), actual.Balances.OrderBy(pair => pair.Key));
            Assert.Equal(expected.Suggestions.Order(), actual.Suggestions.Order());
            Assert.Equal(expected.History.Order(), actual.History.Order());
        }

        public static void SameExpenses(IReadOnlyList<ExpenseSnapshot> expected, IReadOnlyList<ExpenseSnapshot> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            foreach (var expectedExpense in expected)
            {
                var actualExpense = Assert.Single(actual, expense => expense.Description == expectedExpense.Description);
                Assert.Equal(expectedExpense.Total, actualExpense.Total);
                Assert.Equal(expectedExpense.PaidBy, actualExpense.PaidBy);
                Assert.Equal(expectedExpense.Voided, actualExpense.Voided);
                Assert.Equal(expectedExpense.Shares.OrderBy(pair => pair.Key), actualExpense.Shares.OrderBy(pair => pair.Key));
            }
        }
    }
}
