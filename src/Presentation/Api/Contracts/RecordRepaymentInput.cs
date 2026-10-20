using System;

namespace Api.Contracts
{
    public sealed class RecordRepaymentInput
    {
        public Guid GroupKey { get; set; }
        public Guid SenderKey { get; set; }
        public Guid RecipientKey { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal Amount { get; set; }
        public Guid IdempotencyKey { get; set; }
    }
}
