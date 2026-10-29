using System;

namespace Api.Contracts
{
    public sealed class CreateExpenseShareInput
    {
        public Guid ParticipantKey { get; set; }
        public decimal Amount { get; set; }
    }
}
