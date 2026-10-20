namespace Tests.Infrastructure.SqlDatabase
{
    public sealed class MySqlIntegrationFactAttribute : FactAttribute
    {
        public MySqlIntegrationFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EXPENSEWISE_TEST_MYSQL_CONNECTION")))
            {
                Skip = "Set EXPENSEWISE_TEST_MYSQL_CONNECTION to run MySQL integration tests.";
            }
        }
    }
}
