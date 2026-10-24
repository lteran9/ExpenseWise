using Microsoft.Playwright;

namespace Tests.Acceptance
{
    public sealed class BrowserFixture : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        public IBrowser Browser => _browser
            ?? throw new InvalidOperationException("The browser is only started when acceptance tests are configured.");

        public async Task InitializeAsync()
        {
            if (!AcceptanceSettings.IsConfigured)
            {
                return;
            }

            // Downloads Chromium on first use; a no-op when it is already installed.
            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Playwright could not install Chromium (exit code {exitCode}).");
            }

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }

        public async Task DisposeAsync()
        {
            if (_browser is not null)
            {
                await _browser.DisposeAsync();
            }

            _playwright?.Dispose();
        }
    }
}
