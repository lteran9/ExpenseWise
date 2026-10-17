using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator()
        {
            RuleFor(x => x.ActorKey)
               .NotEqual(Guid.Empty)
               .WithMessage("An authenticated user is required.");
        }
    }
}
