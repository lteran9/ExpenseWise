using System.Collections.Generic;
using Core.Entities;

namespace Application.UseCases
{
    public sealed record GroupLedgerSnapshot(GroupLedger Ledger, List<Repayment> Repayments);
}
