namespace Api.Contracts;

public sealed class CreateExpenseInput
{
    public string Description { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid GroupKey { get; set; }
}

public sealed class SplitExpenseInput
{
    public Guid GroupKey { get; set; }
}
