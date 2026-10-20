using System;
using System.Collections.Generic;

namespace Api.Contracts
{
    /// <summary>Expenses and group details shown to a member of a group.</summary>
    public sealed record ExpenseListResponse(
        Guid GroupKey,
        int GroupMembers,
        IReadOnlyList<ExpenseListItemResponse> Expenses);
}
