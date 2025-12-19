using System;

namespace Core.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        public bool Settled { get; set; }

        public string Description { get; set; }
        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public Expense()
        {
            Description = string.Empty;
            Currency = string.Empty;
        }
    }
}
