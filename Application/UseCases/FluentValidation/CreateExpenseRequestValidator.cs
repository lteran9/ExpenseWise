using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
    {
        public CreateExpenseRequestValidator()
        {
            RuleFor(x => x.Description)
               .NotEmpty()
               .WithMessage("Please provide a description for the expense.");
            RuleFor(x => x.Currency)
               .NotEmpty()
               .WithMessage("Please provide a currency type.");
            RuleFor(x => x.Amount)
               .NotEmpty()
               .WithMessage("Please provide an expense amount.");
        }
    }
}
