using System.Net;
using Microsoft.Playwright;

namespace Tests.Acceptance
{
    /// <summary>
    /// Access-boundary portion of product scenario C through the Docker-hosted stack.
    /// </summary>
    [Trait("Category", "Scenario")]
    public sealed class AccessBoundaryTests : IClassFixture<BrowserFixture>
    {
        private readonly BrowserFixture _fixture;

        public AccessBoundaryTests(BrowserFixture fixture)
        {
            _fixture = fixture;
        }

        [AcceptanceFact]
        public async Task AnonymousRequests_ToApiAndUi_RequireAuthentication()
        {
            // Arrange
            using var http = new HttpClient();
            await using var anonymous = await UserSession.OpenAsync(_fixture.Browser, "access-anonymous");

            // Act
            var apiResponse = await http.GetAsync($"{AcceptanceSettings.ApiUrl}/Group/List");
            await anonymous.GotoAsync($"/Expense/List?group={Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
            Assert.Contains("/Auth/SignIn", anonymous.Page.Url);
        }

        [AcceptanceFact]
        public async Task Nonmember_OpeningAnotherGroup_SeesNoGroupData()
        {
            // Arrange
            var runId = Guid.NewGuid().ToString("N")[..8];
            var alice = TestUser.Create("Alice", runId);
            var eve = TestUser.Create("Eve", runId);
            var groupName = $"Private {runId}";
            var expenseName = $"Private dinner {runId}";
            await using var aliceSession = await UserSession.OpenAsync(_fixture.Browser, $"access-nonmember-alice-{runId}");
            await using var eveSession = await UserSession.OpenAsync(_fixture.Browser, $"access-nonmember-eve-{runId}");
            await aliceSession.RegisterAsync(alice);
            await eveSession.RegisterAsync(eve);
            var groupKey = await aliceSession.CreateGroupAsync(groupName);
            await aliceSession.RecordExpenseAsync(groupKey, expenseName, 12.00m, alice, alice);

            // Act & Assert
            await eveSession.GotoAsync("/Group");
            Assert.DoesNotContain(groupName, await eveSession.Page.ContentAsync());

            foreach (var path in new[] { $"/Expense/List?group={groupKey}", $"/Repayment/Ledger?group={groupKey}" })
            {
                await eveSession.GotoAsync(path);
                Assert.NotEmpty(await eveSession.ReadErrorsAsync());
                Assert.DoesNotContain(expenseName, await eveSession.Page.ContentAsync());
                Assert.DoesNotContain(alice.Name, await eveSession.Page.ContentAsync());
            }

            await eveSession.GotoAsync($"/Expense/Create?group={groupKey}");
            Assert.Equal(0, await eveSession.Page.Locator("#Amount").CountAsync());
        }

        [AcceptanceFact]
        public async Task OrdinaryMember_ForgingOrganizerWrites_IsRejectedServerSide()
        {
            // Arrange
            var runId = Guid.NewGuid().ToString("N")[..8];
            var alice = TestUser.Create("Alice", runId);
            var bob = TestUser.Create("Bob", runId);
            await using var aliceSession = await UserSession.OpenAsync(_fixture.Browser, $"access-member-alice-{runId}");
            await using var bobSession = await UserSession.OpenAsync(_fixture.Browser, $"access-member-bob-{runId}");
            await aliceSession.RegisterAsync(alice);
            await bobSession.RegisterAsync(bob);
            var groupKey = await aliceSession.CreateGroupAsync($"Organizer only {runId}");
            await aliceSession.InviteAsync(groupKey, bob);
            await aliceSession.RecordExpenseAsync(groupKey, "Groceries", 20.00m, alice, alice, bob);
            var memberKeys = await aliceSession.ReadMemberKeysAsync(groupKey);
            var expenseKey = Assert.Single(await aliceSession.ReadVoidableExpenseKeysAsync(groupKey));
            var ledgerBefore = await aliceSession.ReadLedgerAsync(groupKey);
            var expensesBefore = await aliceSession.ReadExpensesAsync(groupKey);

            // Act
            await bobSession.GotoAsync($"/Expense/List?group={groupKey}");
            var recordExpenseLinks = await bobSession.Page.GetByRole(AriaRole.Link, new() { Name = "Record expense" }).CountAsync();
            var voidButtons = await bobSession.Page.GetByRole(AriaRole.Button, new() { Name = "Void" }).CountAsync();
            await bobSession.PostFormAsync("/Expense/Void", new Dictionary<string, string>
            {
                ["groupKey"] = groupKey.ToString(),
                ["expenseKey"] = expenseKey
            });
            await bobSession.PostFormAsync("/Repayment/Record", new Dictionary<string, string>
            {
                ["groupKey"] = groupKey.ToString(),
                ["senderKey"] = memberKeys[bob.Name],
                ["recipientKey"] = memberKeys[alice.Name],
                ["amount"] = "10.00",
                ["idempotencyKey"] = Guid.NewGuid().ToString()
            });

            // Assert
            Assert.Equal(0, recordExpenseLinks);
            Assert.Equal(0, voidButtons);
            Assert.False((await bobSession.ReadLedgerAsync(groupKey)).CanRecordRepayment);
            LedgerAssert.Unchanged(ledgerBefore, await aliceSession.ReadLedgerAsync(groupKey));
            LedgerAssert.SameExpenses(expensesBefore, await aliceSession.ReadExpensesAsync(groupKey));
        }
    }
}
