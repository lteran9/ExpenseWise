using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal sealed class RecordRepaymentRequestValidator : AbstractValidator<RecordRepaymentRequest>
    {
        public RecordRepaymentRequestValidator()
        {
            RuleFor(request => request.ActorKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An authenticated user is required.");
            RuleFor(request => request.GroupKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A group key is required.");
            RuleFor(request => request.IdempotencyKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A repayment request key is required.");
            RuleFor(request => request.SenderKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A sender is required.");
            RuleFor(request => request.RecipientKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A recipient is required.");
            RuleFor(request => request.RecipientKey)
                .NotEqual(request => request.SenderKey)
                .WithMessage("The sender and recipient must be different members.");
            RuleFor(request => request.Currency)
                .Equal("USD", StringComparer.OrdinalIgnoreCase)
                .WithMessage("Only USD repayments are supported.");
            RuleFor(request => request.Amount)
                .GreaterThan(0)
                .WithMessage("Repayment amount must be greater than zero.");
            RuleFor(request => request.Amount)
                .Must(amount => decimal.Round(amount, 2) == amount)
                .WithMessage("Repayment amounts may have at most two decimal places.");
            RuleFor(request => request.Amount)
                .LessThanOrEqualTo(9999999999999999.99m)
                .WithMessage("Repayment amount exceeds the supported range.");
        }
    }

    internal sealed class RetrieveGroupLedgerRequestValidator : AbstractValidator<RetrieveGroupLedgerRequest>
    {
        public RetrieveGroupLedgerRequestValidator()
        {
            RuleFor(request => request.ActorKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An authenticated user is required.");
            RuleFor(request => request.GroupKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A group key is required.");
        }
    }
}
