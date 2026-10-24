using System;
using System.Collections.Generic;

namespace UI.Models
{
    public sealed class ExpenseLedgerViewModel
    {
        public Guid GroupKey { get; set; }
        public bool HasLoadedExpenses { get; set; }
        public bool IsOrganizer { get; set; }
        public bool HasRepayments { get; set; }
        public bool CanVoid => IsOrganizer && !HasRepayments;
        public List<ExpenseLedgerItemViewModel> Expenses { get; set; } = [];
    }
}
