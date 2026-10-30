using System.Net;

namespace Tests.Acceptance
{
    [Trait("Category", "Scenario")]
    public sealed class HealthEndpointTests : IClassFixture<BrowserFixture>
    {
        private readonly BrowserFixture _fixture;

        public HealthEndpointTests(BrowserFixture fixture)
        {
            _fixture = fixture;
        }

        [AcceptanceFact]
        public async Task HealthEndpoints_WhenStackIsReady_ReturnSuccessfulResponses()
        {
            using var client = new HttpClient();

            var uiLiveness = await client.GetAsync($"{AcceptanceSettings.UiUrl}/health/live");
            var apiLiveness = await client.GetAsync($"{AcceptanceSettings.ApiUrl}/health/live");
            var apiReadiness = await client.GetAsync($"{AcceptanceSettings.ApiUrl}/health/ready");
            var legacyApiHealth = await client.GetAsync($"{AcceptanceSettings.ApiUrl}/Health");

            Assert.Equal(HttpStatusCode.OK, uiLiveness.StatusCode);
            Assert.Equal(HttpStatusCode.OK, apiLiveness.StatusCode);
            Assert.Equal(HttpStatusCode.OK, apiReadiness.StatusCode);
            Assert.Equal(HttpStatusCode.OK, legacyApiHealth.StatusCode);
            Assert.Equal(
                "API running on .NET 10",
                await legacyApiHealth.Content.ReadAsStringAsync());
        }

        [AcceptanceFact]
        public async Task Ui_WhenProxyForwardsHttps_SetsSecureCookies()
        {
            var runId = Guid.NewGuid().ToString("N")[..8];
            var user = TestUser.Create("Proxy", runId);
            await using var session = await UserSession.OpenAsync(
                _fixture.Browser,
                $"forwarded-https-{runId}",
                forwardHttps: true);

            await session.RegisterAsync(user);

            Assert.True(await session.HasSecureAuthenticationAndSessionCookiesAsync());
        }
    }
}
