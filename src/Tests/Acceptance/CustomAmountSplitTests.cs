namespace Tests.Acceptance
{
    [Trait("Category", "Scenario")]
    public sealed class CustomAmountSplitTests : IClassFixture<BrowserFixture>
    {
        private readonly BrowserFixture _fixture;

        public CustomAmountSplitTests(BrowserFixture fixture)
        {
            _fixture = fixture;
        }

        [AcceptanceFact]
        public async Task CustomAmounts_ExactAndZeroShares_AppearInTheExpenseLedger()
        {
            var runId = Guid.NewGuid().ToString("N")[..8];
            var alice = TestUser.Create("Alice", runId);
            var bob = TestUser.Create("Bob", runId);
            await using var aliceSession = await UserSession.OpenAsync(_fixture.Browser, $"custom-split-alice-{runId}");
            await using var bobSession = await UserSession.OpenAsync(_fixture.Browser, $"custom-split-bob-{runId}");
            await aliceSession.RegisterAsync(alice);
            await bobSession.RegisterAsync(bob);
            var groupKey = await aliceSession.CreateGroupAsync($"Custom split {runId}");
            await aliceSession.InviteAsync(groupKey, bob);

            var dinnerSubmission = await aliceSession.RecordCustomExpenseAsync(
                groupKey,
                "Dinner",
                10.01m,
                alice,
                (alice, 6.00m),
                (bob, 4.01m));
            await aliceSession.RecordCustomExpenseAsync(
                groupKey,
                "Breakfast",
                10.00m,
                alice,
                (alice, 10.00m),
                (bob, 0m));
            await aliceSession.PostFormAsync("/Expense/Create", dinnerSubmission);

            var expenses = await aliceSession.ReadExpensesAsync(groupKey);
            Assert.Equal(2, expenses.Count);
            Assert.Equal(
                new Dictionary<string, string> { [alice.Name] = "$6.00", [bob.Name] = "$4.01" },
                Assert.Single(expenses, expense => expense.Description == "Dinner").Shares);
            Assert.Equal(
                new Dictionary<string, string> { [alice.Name] = "$10.00", [bob.Name] = "$0.00" },
                Assert.Single(expenses, expense => expense.Description == "Breakfast").Shares);
        }
    }
}
