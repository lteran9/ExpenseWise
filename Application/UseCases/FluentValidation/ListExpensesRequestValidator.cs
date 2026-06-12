using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class ListExpensesRequestValidator : AbstractValidator<ListExpensesRequest>
    {
        public ListExpensesRequestValidator()
        {
            RuleFor(x => x.GroupKey)
               .NotEqual(Guid.Empty)
               .WithMessage("Please provide a group to query with.");
        }
    }
}
