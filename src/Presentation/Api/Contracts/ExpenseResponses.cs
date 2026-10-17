namespace Api.Contracts;

/// <summary>Expenses and group details shown to a member of a group.</summary>
public sealed record ExpenseListResponse(
    Guid GroupKey,
    int GroupMembers,
    IReadOnlyList<ExpenseListItemResponse> Expenses);

/// <summary>An expense included in a group's expense list.</summary>
public sealed record ExpenseListItemResponse(
    int Id,
    string Description,
    string Currency,
    decimal Amount);
