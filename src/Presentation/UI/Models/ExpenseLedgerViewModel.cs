using System;
using System.Collections.Generic;

namespace UI.Models
{
    public sealed class ExpenseLedgerViewModel
    {
        public Guid GroupKey { get; set; }
        public bool HasLoadedExpenses { get; set; }
        public bool IsOrganizer { get; set; }
        public List<ExpenseLedgerItemViewModel> Expenses { get; set; } = [];
    }
}
