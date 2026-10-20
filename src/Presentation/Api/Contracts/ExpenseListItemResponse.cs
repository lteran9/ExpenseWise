using System;
using System.Collections.Generic;

namespace Api.Contracts
{
    /// <summary>An expense included in a group's expense list.</summary>
    public sealed record ExpenseListItemResponse(
        Guid UniqueKey,
        int Id,
        string Description,
        string Currency,
        decimal Amount,
        Guid PayerKey,
        string PayerName,
        Guid RecordedByKey,
        string RecordedByName,
        bool Voided,
        IReadOnlyList<ExpenseShareResponse> Shares);
}
