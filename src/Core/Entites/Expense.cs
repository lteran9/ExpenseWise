using System;

namespace Core.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        public string Description { get; set; }
        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public Guid UniqueKey { get; set; }
        public bool Voided { get; set; }

        public User? CreatedBy { get; set; }
        public User? Payer { get; set; }
        public Group? BelongsTo { get; set; }
        public List<ExpenseShare> Shares { get; set; }

        public Expense()
        {
            Description = string.Empty;
            Currency = string.Empty;
            Shares = new List<ExpenseShare>();
        }
    }
}
