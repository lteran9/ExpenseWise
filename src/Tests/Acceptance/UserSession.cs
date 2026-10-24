using System.Globalization;
using Microsoft.Playwright;

namespace Tests.Acceptance
{
    /// <summary>
    /// One browser identity with its own cookies, driving the ExpenseWise UI the way a person would.
    /// </summary>
    public sealed class UserSession : IAsyncDisposable
    {
        private readonly IBrowserContext _context;
        private readonly string _traceName;

        private UserSession(IBrowserContext context, IPage page, string traceName)
        {
            _context = context;
            _traceName = traceName;
            Page = page;
        }

        public IPage Page { get; }

        public static async Task<UserSession> OpenAsync(IBrowser browser, string traceName)
        {
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = AcceptanceSettings.UiUrl,
                Locale = "en-US"
            });
            await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true });

            var page = await context.NewPageAsync();
            page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            return new UserSession(context, page, traceName);
        }

        public async Task RegisterAsync(TestUser user)
        {
            await GotoAsync("/Auth/Register");
            await Page.Locator("#FirstName").FillAsync(user.FirstName);
            await Page.Locator("#LastName").FillAsync(user.LastName);
            await Page.Locator("#Email").FillAsync(user.Email);
            await Page.Locator("#CountryCode").SelectOptionAsync("+1");
            await Page.Locator("#Phone").FillAsync(user.Phone);
            await Page.Locator("#Password").FillAsync(user.Password);
            await Page.Locator("#ConfirmPassword").FillAsync(user.Password);
            await SubmitAsync("Register");
            await AssertSignedInAsync();
        }

        public async Task SignInAsync(string email, string password)
        {
            await GotoAsync("/Auth/SignIn");
            await Page.Locator("#Email").FillAsync(email);
            await Page.Locator("#Password").FillAsync(password);
            await SubmitAsync("Submit");
            await AssertSignedInAsync();
        }

        public async Task<Guid> CreateGroupAsync(string name)
        {
            var today = DateTime.Today;
            await GotoAsync("/Group/Create");
            await Page.Locator("#Name").FillAsync(name);
            await Page.Locator("#StartDate").FillAsync(today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            await Page.Locator("#EndDate").FillAsync(today.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            await SubmitAsync("Create");

            var card = Page.Locator(".card", new PageLocatorOptions
            {
                Has = Page.Locator("h2", new PageLocatorOptions { HasTextString = name })
            });
            var href = await card.GetByLabel("View expenses").GetAttributeAsync("href")
                ?? throw new InvalidOperationException($"Group '{name}' has no expense link.");
            return Guid.Parse(new Uri(new Uri(AcceptanceSettings.UiUrl), href).Query.Split('=')[1]);
        }

        public async Task InviteAsync(Guid groupKey, TestUser member)
        {
            await GotoAsync($"/Group/Invite?key={groupKey}");
            await Page.Locator("#CountryCode").SelectOptionAsync("+1");
            await Page.Locator("#Phone").FillAsync(member.Phone);
            await SubmitAsync("Send");
        }

        public async Task RecordExpenseAsync(
            Guid groupKey, string description, decimal amount, TestUser payer, params TestUser[] participants)
        {
            await GotoAsync($"/Expense/Create?group={groupKey}");
            await Page.Locator("#Description").FillAsync(description);
            await Page.Locator("#Amount").FillAsync(Money(amount));
            await Page.Locator("#PayerKey").SelectOptionAsync(new SelectOptionValue { Label = payer.Name });
            foreach (var checkbox in await Page.Locator("input[name=ParticipantKeys]").AllAsync())
            {
                await checkbox.UncheckAsync();
            }

            foreach (var participant in participants)
            {
                await Page.GetByLabel(participant.Name, new PageGetByLabelOptions { Exact = true }).CheckAsync();
            }

            await SubmitAsync("Add Expense");
        }

        /// <summary>
        /// Records a repayment through the confirmation dialog and returns the exact form body that was submitted,
        /// so a test can replay the same submission.
        /// </summary>
        public async Task<string> RecordRepaymentAsync(Guid groupKey, TestUser sender, TestUser recipient, decimal amount)
        {
            await GotoAsync($"/Repayment/Ledger?group={groupKey}");
            await Page.Locator("#senderKey").SelectOptionAsync(new SelectOptionValue { Label = sender.Name });
            await Page.Locator("#recipientKey").SelectOptionAsync(new SelectOptionValue { Label = recipient.Name });
            await Page.Locator("#amount").FillAsync(Money(amount));
            var body = await Page.EvaluateAsync<string>(
                "() => new URLSearchParams(new FormData(document.querySelector('form[action=\"/Repayment/Record\"]'))).toString()");
            await SubmitAsync("Record repayment");
            return body;
        }

        /// <summary>
        /// Posts a raw form body from this browser, as a resubmission or a hand-crafted request would, and returns the final HTML.
        /// </summary>
        public async Task<string> PostFormAsync(string path, string body) =>
            await Page.EvaluateAsync<string>(
                @"async ([path, body]) => {
                    const response = await fetch(path, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body
                    });
                    return await response.text();
                }",
                new[] { path, body });

        public async Task<string> PostFormAsync(string path, IDictionary<string, string> fields)
        {
            var values = new Dictionary<string, string>(fields)
            {
                ["__RequestVerificationToken"] = await Page.Locator("input[name=__RequestVerificationToken]").First.InputValueAsync()
            };
            using var content = new FormUrlEncodedContent(values);
            return await PostFormAsync(path, await content.ReadAsStringAsync());
        }

        public async Task<LedgerSnapshot> ReadLedgerAsync(Guid groupKey)
        {
            await GotoAsync($"/Repayment/Ledger?group={groupKey}");
            await AssertNoErrorsAsync();

            var balances = new Dictionary<string, string>();
            foreach (var item in await Section("Member balances").Locator("li").AllAsync())
            {
                balances[await item.Locator("span").First.InnerTextAsync()] = await item.Locator("span").Last.InnerTextAsync();
            }

            return new LedgerSnapshot(
                await SummaryValueAsync("Total spent"),
                await SummaryValueAsync("Outstanding"),
                balances,
                Trim(await Section("Suggested repayments").Locator("li").AllInnerTextsAsync()),
                Trim(await Section("Repayment history").Locator("li span").AllInnerTextsAsync()),
                await Section("Record a repayment").CountAsync() > 0);
        }

        public async Task<IReadOnlyList<ExpenseSnapshot>> ReadExpensesAsync(Guid groupKey)
        {
            await GotoAsync($"/Expense/List?group={groupKey}");
            await AssertNoErrorsAsync();

            var expenses = new List<ExpenseSnapshot>();
            foreach (var card in await Page.Locator("main section.card").AllAsync())
            {
                var shares = new Dictionary<string, string>();
                foreach (var share in await card.Locator("li").AllAsync())
                {
                    shares[await share.Locator("span").First.InnerTextAsync()] = await share.Locator("span").Last.InnerTextAsync();
                }

                var paidBy = await card.Locator("p", new LocatorLocatorOptions { HasTextString = "Paid by " }).InnerTextAsync();
                expenses.Add(new ExpenseSnapshot(
                    (await card.Locator("h2").InnerTextAsync()).Trim(),
                    (await card.Locator("p", new LocatorLocatorOptions { HasTextString = "Total: " }).InnerTextAsync()).Trim(),
                    paidBy["Paid by ".Length..paidBy.IndexOf(';')],
                    await card.Locator(".badge", new LocatorLocatorOptions { HasTextString = "Voided" }).CountAsync() > 0,
                    shares));
            }

            return expenses;
        }

        public async Task<IReadOnlyDictionary<string, string>> ReadMemberKeysAsync(Guid groupKey)
        {
            await GotoAsync($"/Repayment/Ledger?group={groupKey}");
            var keys = new Dictionary<string, string>();
            foreach (var option in await Page.Locator("#senderKey option:not([value=''])").AllAsync())
            {
                keys[await option.InnerTextAsync()] = await option.GetAttributeAsync("value") ?? string.Empty;
            }

            return keys;
        }

        public async Task<IReadOnlyList<string>> ReadVoidableExpenseKeysAsync(Guid groupKey)
        {
            await GotoAsync($"/Expense/List?group={groupKey}");
            var keys = new List<string>();
            foreach (var input in await Page.Locator("form[action='/Expense/Void'] input[name=expenseKey]").AllAsync())
            {
                keys.Add(await input.InputValueAsync());
            }

            return keys;
        }

        public async Task GotoAsync(string path)
        {
            var response = await Page.GotoAsync(path);
            Assert.True(response?.Ok ?? false, $"GET {path} returned HTTP {response?.Status}.");
        }

        public async Task<IReadOnlyList<string>> ReadErrorsAsync() =>
            Trim(await Page.Locator(".validation-summary-errors li, .alert-danger").AllInnerTextsAsync());

        public async ValueTask DisposeAsync()
        {
            Directory.CreateDirectory(AcceptanceSettings.ArtifactsDirectory);
            await _context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(AcceptanceSettings.ArtifactsDirectory, $"{_traceName}.zip")
            });
            await _context.DisposeAsync();
        }

        private async Task SubmitAsync(string buttonName)
        {
            var button = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = buttonName, Exact = true });
            var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnLoad(object? sender, IPage page) => loaded.TrySetResult();

            // Form posts redirect back to the same URL, so wait for the next document load rather than a URL change.
            Page.Load += OnLoad;
            try
            {
                await button.ClickAsync();
                await loaded.Task.WaitAsync(TimeSpan.FromSeconds(30));
            }
            finally
            {
                Page.Load -= OnLoad;
            }

            await AssertNoErrorsAsync();
        }

        private async Task AssertNoErrorsAsync()
        {
            var errors = await ReadErrorsAsync();
            Assert.True(errors.Count == 0, $"{Page.Url} showed errors: {string.Join(" | ", errors)}");
        }

        private async Task AssertSignedInAsync() =>
            Assert.Equal(1, await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log Out" }).CountAsync());

        private ILocator Section(string heading) =>
            Page.Locator("section", new PageLocatorOptions
            {
                Has = Page.Locator("h2", new PageLocatorOptions { HasTextString = heading })
            });

        private async Task<string> SummaryValueAsync(string heading) =>
            (await Page.Locator(".card-body", new PageLocatorOptions
            {
                Has = Page.Locator("h2", new PageLocatorOptions { HasTextString = heading })
            }).Locator("p").InnerTextAsync()).Trim();

        private static string Money(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

        private static IReadOnlyList<string> Trim(IEnumerable<string> values) =>
            values.Select(value => value.Trim()).Where(value => value.Length > 0).ToList();
    }
}
