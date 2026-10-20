using System;

namespace Api.Contracts
{
    public sealed class VoidExpenseInput
    {
        public Guid GroupKey { get; set; }
        public Guid ExpenseKey { get; set; }
    }
}
