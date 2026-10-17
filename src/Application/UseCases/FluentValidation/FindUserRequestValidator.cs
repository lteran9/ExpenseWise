using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class FindUserRequestValidator : AbstractValidator<FindUserRequest>
    {
        public FindUserRequestValidator()
        {
            RuleFor(x => x.ActorKey)
               .NotEqual(Guid.Empty)
               .WithMessage("An authenticated user is required.");
        }
    }
}
