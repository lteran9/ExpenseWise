using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class RetrieveGroupLedger : BaseRequestHandler<RetrieveGroupLedgerRequest, RetrieveGroupLedgerResponse>
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IRepaymentRepository _repaymentRepository;
        private readonly AbstractValidator<RetrieveGroupLedgerRequest> _validator;

        public RetrieveGroupLedger(
            IGroupRepository groupRepository,
            IRepaymentRepository repaymentRepository)
        {
            _groupRepository = groupRepository;
            _repaymentRepository = repaymentRepository;
            _validator = new RetrieveGroupLedgerRequestValidator();
        }

        public override async Task<ResponseWrapper<RetrieveGroupLedgerResponse>> Handle(RetrieveGroupLedgerRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
            }

            var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
            if (group == null)
            {
                return Invalid("Unable to find a group for the given key.");
            }

            if (!group.Members.Any(member => member.UniqueKey == request.ActorKey))
            {
                return Invalid("Only group members can view the ledger.");
            }

            var snapshot = await _repaymentRepository.GetGroupLedgerSnapshotAsync(group.Id, cancellationToken);
            var ledger = snapshot.Ledger;
            var repayments = snapshot.Repayments;

            return Successful(new RetrieveGroupLedgerResponse
            {
                TotalSpent = ledger.TotalSpent,
                Outstanding = ledger.Outstanding,
                Balances = ledger.Balances.Select(balance => new MemberBalanceResponse
                {
                    UniqueKey = balance.Member.UniqueKey,
                    Name = balance.Member.Name,
                    NetBalance = balance.NetBalance
                }).ToList(),
                Suggestions = ledger.SuggestRepayments().Select(suggestion => new RepaymentSuggestionResponse
                {
                    FromKey = suggestion.From.UniqueKey,
                    FromName = suggestion.From.Name,
                    ToKey = suggestion.To.UniqueKey,
                    ToName = suggestion.To.Name,
                    Amount = suggestion.Amount
                }).ToList(),
                Repayments = repayments
                    .OrderByDescending(repayment => repayment.OccurredAt)
                    .Select(repayment => new RepaymentHistoryResponse
                    {
                        RepaymentKey = repayment.UniqueKey,
                        SenderName = repayment.Sender?.Name ?? string.Empty,
                        RecipientName = repayment.Recipient?.Name ?? string.Empty,
                        Amount = repayment.Amount,
                        Currency = repayment.Currency,
                        OccurredAt = repayment.OccurredAt
                    }).ToList()
            });
        }
    }

    public class RetrieveGroupLedgerRequest : IRequest<ResponseWrapper<RetrieveGroupLedgerResponse>>
    {
        public Guid ActorKey { get; set; }
        public Guid GroupKey { get; set; }
    }

    public class RetrieveGroupLedgerResponse
    {
        public decimal TotalSpent { get; set; }
        public decimal Outstanding { get; set; }
        public List<MemberBalanceResponse> Balances { get; set; } = new();
        public List<RepaymentSuggestionResponse> Suggestions { get; set; } = new();
        public List<RepaymentHistoryResponse> Repayments { get; set; } = new();
    }

    public class MemberBalanceResponse
    {
        public Guid UniqueKey { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal NetBalance { get; set; }
    }

    public class RepaymentSuggestionResponse
    {
        public Guid FromKey { get; set; }
        public string FromName { get; set; } = string.Empty;
        public Guid ToKey { get; set; }
        public string ToName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class RepaymentHistoryResponse
    {
        public Guid RepaymentKey { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}
