using System.Net.Http.Headers;

namespace UI.Configuration;

public sealed class ApiTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    public const string SessionKey = "ExpenseWise.ApiAccessToken";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
