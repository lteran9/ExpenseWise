using System;
using System.Collections.Generic;

namespace UI.Models
{
    public sealed class ExpenseLedgerItemViewModel
    {
        public Guid Key { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PayerName { get; set; } = string.Empty;
        public string RecordedByName { get; set; } = string.Empty;
        public bool Voided { get; set; }
        public List<ExpenseShareViewModel> Shares { get; set; } = [];
    }
}
