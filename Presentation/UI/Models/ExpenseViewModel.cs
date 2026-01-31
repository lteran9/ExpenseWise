using System;

namespace UI.Models
{
    public class ExpenseViewModel
    {
        public string Currency { get; set; }
        public string Description { get; set; }

        public decimal Amount { get; set; }

        public Guid GroupKey { get; set; }

        public ExpenseViewModel()
        {
            Currency = "USD";
            Description = string.Empty;
        }
    }
}
