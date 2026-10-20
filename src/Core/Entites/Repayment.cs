using System;

namespace Core.Entities
{
    public class Repayment
    {
        public int Id { get; set; }

        public Guid UniqueKey { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime OccurredAt { get; set; }

        public Group? BelongsTo { get; set; }
        public User? Sender { get; set; }
        public User? Recipient { get; set; }
        public User? RecordedBy { get; set; }

        public Repayment()
        {
            Currency = string.Empty;
        }
    }
}
