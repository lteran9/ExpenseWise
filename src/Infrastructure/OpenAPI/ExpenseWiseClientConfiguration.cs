namespace OpenAPI;

public partial class ExpenseWiseClient
{
    partial void Initialize()
    {
        if (_httpClient.BaseAddress is not null)
        {
            _baseUrl = _httpClient.BaseAddress.ToString();
        }
    }
}
