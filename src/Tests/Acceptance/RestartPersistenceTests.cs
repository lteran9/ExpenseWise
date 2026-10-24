namespace Tests.Acceptance
{
    /// <summary>
    /// Product scenario A step 8: run after the stack is stopped and started again with its existing volume.
    /// </summary>
    [Trait("Category", "AfterRestart")]
    public sealed class RestartPersistenceTests : IClassFixture<BrowserFixture>
    {
        private readonly BrowserFixture _fixture;

        public RestartPersistenceTests(BrowserFixture fixture)
        {
            _fixture = fixture;
        }

        [AcceptanceFact]
        public async Task ScenarioA_AfterStackRestart_KeepsExpensesSharesRepaymentsAndZeroBalances()
        {
            // Arrange
            var expected = await ScenarioAState.LoadAsync();
            await using var aliceSession = await UserSession.OpenAsync(_fixture.Browser, "after-restart-alice");
            await aliceSession.SignInAsync(expected.OrganizerEmail, expected.OrganizerPassword);

            // Act
            var ledger = await aliceSession.ReadLedgerAsync(expected.GroupKey);
            var expenses = await aliceSession.ReadExpensesAsync(expected.GroupKey);

            // Assert
            LedgerAssert.Unchanged(expected.Ledger, ledger);
            Assert.Equal("$0.00 USD", ledger.Outstanding);
            Assert.All(ledger.Balances.Values, status => Assert.Equal("Settled up", status));
            LedgerAssert.SameExpenses(expected.Expenses, expenses);
        }
    }
}
