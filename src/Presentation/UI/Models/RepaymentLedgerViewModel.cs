using System;
using System.Collections.Generic;

namespace UI.Models
{
    public sealed class RepaymentLedgerViewModel
    {
        public Guid GroupKey { get; set; }
        public Guid IdempotencyKey { get; set; }
        public bool HasLoadedLedger { get; set; }
        public bool IsOrganizer { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Outstanding { get; set; }
        public List<ExpenseMemberOptionViewModel> Members { get; set; } = [];
        public List<MemberBalanceViewModel> Balances { get; set; } = [];
        public List<RepaymentSuggestionViewModel> Suggestions { get; set; } = [];
        public List<RepaymentHistoryViewModel> History { get; set; } = [];
    }

    public sealed class MemberBalanceViewModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal NetBalance { get; set; }
    }

    public sealed class RepaymentSuggestionViewModel
    {
        public string FromName { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public sealed class RepaymentHistoryViewModel
    {
        public string SenderName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}
