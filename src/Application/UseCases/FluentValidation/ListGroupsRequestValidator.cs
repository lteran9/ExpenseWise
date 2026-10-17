using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class ListGroupsRequestValidator : AbstractValidator<ListGroupsRequest>
    {
        public ListGroupsRequestValidator()
        {
            RuleFor(x => x.ActorKey)
               .NotEqual(Guid.Empty)
               .WithMessage("An authenticated user is required.");
        }
    }
}
