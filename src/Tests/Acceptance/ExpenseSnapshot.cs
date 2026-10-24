namespace Tests.Acceptance
{
    public sealed record ExpenseSnapshot(
        string Description,
        string Total,
        string PaidBy,
        bool Voided,
        IReadOnlyDictionary<string, string> Shares);
}
