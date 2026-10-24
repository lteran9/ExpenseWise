namespace Tests.Acceptance
{
    public sealed record TestUser(string FirstName, string LastName, string Email, string Phone, string Password)
    {
        public string Name => $"{FirstName} {LastName}";

        public static TestUser Create(string firstName, string runId) => new(
            firstName,
            "Acceptance",
            $"{firstName.ToLowerInvariant()}.{runId}@example.test",
            $"555{Random.Shared.Next(0, 10_000_000):D7}",
            "Accept@nce1");
    }
}
