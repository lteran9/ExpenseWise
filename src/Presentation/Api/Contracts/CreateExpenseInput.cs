using System;
using System.Collections.Generic;

namespace Api.Contracts
{
    public sealed class CreateExpenseInput
    {
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid GroupKey { get; set; }
        public Guid PayerKey { get; set; }
        public List<Guid> ParticipantKeys { get; set; } = [];
        public bool UseCustomAmounts { get; set; }
        public List<CreateExpenseShareInput> CustomShares { get; set; } = [];
        public Guid IdempotencyKey { get; set; }
    }
}
