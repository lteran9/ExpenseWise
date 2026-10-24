namespace Tests.Acceptance
{
    public static class AcceptanceSettings
    {
        public const string UiUrlVariable = "EXPENSEWISE_UI_URL";
        public const string ApiUrlVariable = "EXPENSEWISE_API_URL";
        public const string StateFileVariable = "EXPENSEWISE_ACCEPTANCE_STATE";
        public const string ArtifactsVariable = "EXPENSEWISE_ACCEPTANCE_ARTIFACTS";

        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UiUrlVariable))
            && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ApiUrlVariable));

        public static string UiUrl => Required(UiUrlVariable);

        public static string ApiUrl => Required(ApiUrlVariable);

        public static string StateFile =>
            Environment.GetEnvironmentVariable(StateFileVariable) is { Length: > 0 } path
                ? path
                : Path.Combine(Path.GetTempPath(), "expensewise-acceptance-state.json");

        public static string ArtifactsDirectory =>
            Environment.GetEnvironmentVariable(ArtifactsVariable) is { Length: > 0 } path
                ? path
                : Path.Combine(AppContext.BaseDirectory, "acceptance-artifacts");

        private static string Required(string variable) =>
            Environment.GetEnvironmentVariable(variable)?.TrimEnd('/')
                ?? throw new InvalidOperationException($"Set {variable} to run acceptance tests.");
    }
}
