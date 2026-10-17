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
            RuleFor(x => x.ActorKey)
               .NotEqual(Guid.Empty)
               .WithMessage("An authenticated user is required.");
        }
    }
}
