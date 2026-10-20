using System;

namespace Api.Contracts
{
    public sealed record ExpenseShareResponse(Guid ParticipantKey, string ParticipantName, decimal Amount);
}
