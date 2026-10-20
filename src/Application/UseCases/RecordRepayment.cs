using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class RecordRepayment : BaseRequestHandler<RecordRepaymentRequest, RecordRepaymentResponse>
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IRepaymentRepository _repaymentRepository;
        private readonly AbstractValidator<RecordRepaymentRequest> _validator;

        public RecordRepayment(IGroupRepository groupRepository, IRepaymentRepository repaymentRepository)
        {
            _groupRepository = groupRepository;
            _repaymentRepository = repaymentRepository;
            _validator = new RecordRepaymentRequestValidator();
        }

        public override async Task<ResponseWrapper<RecordRepaymentResponse>> Handle(RecordRepaymentRequest request, CancellationToken cancellationToken)
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

            if (group.Owner?.UniqueKey != request.ActorKey)
            {
                return Invalid("Only the group organizer can record repayments.");
            }

            var sender = group.Members.FirstOrDefault(member => member.UniqueKey == request.SenderKey);
            if (sender == null)
            {
                return Invalid("The sender must be an active member of the group.");
            }

            var recipient = group.Members.FirstOrDefault(member => member.UniqueKey == request.RecipientKey);
            if (recipient == null)
            {
                return Invalid("The recipient must be an active member of the group.");
            }

            var candidate = new Repayment
            {
                UniqueKey = request.IdempotencyKey,
                Currency = request.Currency.ToUpperInvariant(),
                Amount = request.Amount,
                OccurredAt = DateTime.UtcNow,
                BelongsTo = group,
                Sender = sender,
                Recipient = recipient,
                RecordedBy = group.Owner
            };

            var outcome = await _repaymentRepository.RecordAsync(
                candidate,
                ledger => ledger.ValidateRepayment(request.SenderKey, request.RecipientKey, request.Amount),
                cancellationToken);

            if (outcome.RejectionReason != null)
            {
                return Invalid(outcome.RejectionReason);
            }

            if (outcome.Repayment == null)
            {
                return Failed(default);
            }

            if (!outcome.Created && !IsSameSubmission(outcome.Repayment, candidate))
            {
                return Invalid("This repayment key was already used for a different repayment.");
            }

            return Successful(new RecordRepaymentResponse
            {
                RepaymentKey = outcome.Repayment.UniqueKey,
                Created = outcome.Created
            });
        }

        private static bool IsSameSubmission(Repayment stored, Repayment requested)
        {
            return stored.Currency == requested.Currency
                && stored.Amount == requested.Amount
                && stored.BelongsTo?.UniqueKey == requested.BelongsTo?.UniqueKey
                && stored.Sender?.UniqueKey == requested.Sender?.UniqueKey
                && stored.Recipient?.UniqueKey == requested.Recipient?.UniqueKey;
        }
    }

    public class RecordRepaymentRequest : IRequest<ResponseWrapper<RecordRepaymentResponse>>
    {
        public Guid ActorKey { get; set; }
        public Guid GroupKey { get; set; }
        public Guid IdempotencyKey { get; set; }
        public Guid SenderKey { get; set; }
        public Guid RecipientKey { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal Amount { get; set; }
    }

    public class RecordRepaymentResponse
    {
        public Guid RepaymentKey { get; set; }
        public bool Created { get; set; }
    }
}
