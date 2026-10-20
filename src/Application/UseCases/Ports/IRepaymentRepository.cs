using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IRepaymentRepository
    {
        Task<List<Repayment>> GetGroupRepaymentsAsync(int groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Locks the group, then either returns the repayment already stored under the same unique key or
        /// validates the candidate against the current ledger and stores it. Both steps use one locked state.
        /// </summary>
        Task<RecordRepaymentOutcome> RecordAsync(
            Repayment repayment,
            Func<GroupLedger, string?> validateAgainstLedger,
            CancellationToken cancellationToken);
    }

    public sealed record RecordRepaymentOutcome(Repayment? Repayment, bool Created, string? RejectionReason);
}
