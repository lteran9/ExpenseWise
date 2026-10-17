using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    public class SplitExpenseRequestValidator : AbstractValidator<SplitExpenseRequest>
    {
        public SplitExpenseRequestValidator()
        {
            RuleFor(x => x.GroupKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a group key.");

            RuleFor(x => x.ActorKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An authenticated user is required.");
        }
    }
}