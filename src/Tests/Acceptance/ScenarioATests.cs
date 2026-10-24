namespace Tests.Acceptance
{
    /// <summary>
    /// Product scenario A (docs/product/README.md) through the Docker-hosted browser UI.
    /// </summary>
    [Trait("Category", "Scenario")]
    public sealed class ScenarioATests : IClassFixture<BrowserFixture>
    {
        private readonly BrowserFixture _fixture;

        public ScenarioATests(BrowserFixture fixture)
        {
            _fixture = fixture;
        }

        [AcceptanceFact]
        public async Task ScenarioA_ExpenseToSettlementJourney_ReconcilesExactlyAtEveryStep()
        {
            // Arrange
            var runId = Guid.NewGuid().ToString("N")[..8];
            var alice = TestUser.Create("Alice", runId);
            var bob = TestUser.Create("Bob", runId);
            var cara = TestUser.Create("Cara", runId);
            await using var aliceSession = await UserSession.OpenAsync(_fixture.Browser, $"scenario-a-alice-{runId}");
            await using var bobSession = await UserSession.OpenAsync(_fixture.Browser, $"scenario-a-bob-{runId}");
            await using var caraSession = await UserSession.OpenAsync(_fixture.Browser, $"scenario-a-cara-{runId}");
            await aliceSession.RegisterAsync(alice);
            await bobSession.RegisterAsync(bob);
            await caraSession.RegisterAsync(cara);
            var groupKey = await aliceSession.CreateGroupAsync($"Scenario A {runId}");
            await aliceSession.InviteAsync(groupKey, bob);
            await aliceSession.InviteAsync(groupKey, cara);

            // Act & Assert, step 1: Alice pays 90.00 shared by all three.
            await aliceSession.RecordExpenseAsync(groupKey, "Dinner", 90.00m, alice, alice, bob, cara);
            LedgerAssert.Balances(
                await aliceSession.ReadLedgerAsync(groupKey),
                (alice, "Is owed $60.00"), (bob, "Owes $30.00"), (cara, "Owes $30.00"));

            // Step 2: Bob pays 30.00 shared by Alice and Bob; persisted shares are exact.
            await aliceSession.RecordExpenseAsync(groupKey, "Taxi", 30.00m, bob, alice, bob);
            var expenses = await aliceSession.ReadExpensesAsync(groupKey);
            var dinner = Assert.Single(expenses, expense => expense.Description == "Dinner");
            Assert.Equal(new Dictionary<string, string> { [alice.Name] = "$30.00", [bob.Name] = "$30.00", [cara.Name] = "$30.00" }, dinner.Shares);
            var taxi = Assert.Single(expenses, expense => expense.Description == "Taxi");
            Assert.Equal(new Dictionary<string, string> { [alice.Name] = "$15.00", [bob.Name] = "$15.00" }, taxi.Shares);
            Assert.Equal(bob.Name, taxi.PaidBy);

            // Step 3: totals and balances.
            var ledger = await aliceSession.ReadLedgerAsync(groupKey);
            Assert.Equal("$120.00 USD", ledger.TotalSpent);
            Assert.Equal("$45.00 USD", ledger.Outstanding);
            LedgerAssert.Balances(ledger, (alice, "Is owed $45.00"), (bob, "Owes $15.00"), (cara, "Owes $30.00"));

            // Step 4: suggestions; an ordinary member sees the same ledger without organizer actions.
            Assert.Equal(
                new[] { $"{bob.Name} pays {alice.Name} $15.00 USD", $"{cara.Name} pays {alice.Name} $30.00 USD" }.Order(),
                ledger.Suggestions.Order());
            Assert.True(ledger.CanRecordRepayment);
            var bobLedger = await bobSession.ReadLedgerAsync(groupKey);
            LedgerAssert.Unchanged(ledger, bobLedger);
            Assert.False(bobLedger.CanRecordRepayment);

            // Step 5: Bob repays 5.00 through the confirmation dialog.
            var repaymentSubmission = await aliceSession.RecordRepaymentAsync(groupKey, bob, alice, 5.00m);
            var afterRepayment = await aliceSession.ReadLedgerAsync(groupKey);
            Assert.Equal("$40.00 USD", afterRepayment.Outstanding);
            LedgerAssert.Balances(afterRepayment, (alice, "Is owed $40.00"), (bob, "Owes $10.00"), (cara, "Owes $30.00"));
            Assert.Equal(new[] { $"{bob.Name} paid {alice.Name} $5.00 USD" }, afterRepayment.History);

            // Step 6: replaying the same submission changes nothing.
            await aliceSession.PostFormAsync("/Repayment/Record", repaymentSubmission);
            LedgerAssert.Unchanged(afterRepayment, await aliceSession.ReadLedgerAsync(groupKey));

            // Step 7: the remaining repayments settle everyone; total spent is unchanged.
            await aliceSession.RecordRepaymentAsync(groupKey, bob, alice, 10.00m);
            await aliceSession.RecordRepaymentAsync(groupKey, cara, alice, 30.00m);
            var settled = await aliceSession.ReadLedgerAsync(groupKey);
            Assert.Equal("$120.00 USD", settled.TotalSpent);
            Assert.Equal("$0.00 USD", settled.Outstanding);
            LedgerAssert.Balances(settled, (alice, "Settled up"), (bob, "Settled up"), (cara, "Settled up"));
            Assert.Empty(settled.Suggestions);
            Assert.Equal(3, settled.History.Count);

            // Step 8 runs after a stack restart in RestartPersistenceTests.
            await new ScenarioAState(alice.Email, alice.Password, groupKey, settled, await aliceSession.ReadExpensesAsync(groupKey))
                .SaveAsync();
        }
    }
}
