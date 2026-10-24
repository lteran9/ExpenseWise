namespace Tests.Acceptance
{
    public sealed record LedgerSnapshot(
        string TotalSpent,
        string Outstanding,
        IReadOnlyDictionary<string, string> Balances,
        IReadOnlyList<string> Suggestions,
        IReadOnlyList<string> History,
        bool CanRecordRepayment);
}
