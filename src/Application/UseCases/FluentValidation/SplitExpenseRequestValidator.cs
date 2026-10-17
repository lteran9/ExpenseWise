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

            RuleFor(x => x.UserKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a user key.");
        }
    }
}