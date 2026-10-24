using System.Text.Json;

namespace Tests.Acceptance
{
    /// <summary>
    /// Hands scenario A's organizer, group, and final ledger to the post-restart check, which runs as a separate test invocation.
    /// </summary>
    public sealed record ScenarioAState(
        string OrganizerEmail,
        string OrganizerPassword,
        Guid GroupKey,
        LedgerSnapshot Ledger,
        IReadOnlyList<ExpenseSnapshot> Expenses)
    {
        public async Task SaveAsync() =>
            await File.WriteAllTextAsync(AcceptanceSettings.StateFile, JsonSerializer.Serialize(this));

        public static async Task<ScenarioAState> LoadAsync()
        {
            if (!File.Exists(AcceptanceSettings.StateFile))
            {
                throw new InvalidOperationException(
                    $"No scenario A state at {AcceptanceSettings.StateFile}. Run the Scenario category before AfterRestart.");
            }

            return JsonSerializer.Deserialize<ScenarioAState>(await File.ReadAllTextAsync(AcceptanceSettings.StateFile))
                ?? throw new InvalidOperationException("The scenario A state file is empty.");
        }
    }
}
