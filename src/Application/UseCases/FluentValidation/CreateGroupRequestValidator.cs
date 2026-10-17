using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
    {
        public CreateGroupRequestValidator()
        {
            RuleFor(x => x.ActorKey)
               .NotEqual(Guid.Empty)
               .WithMessage("An authenticated user is required.");
            RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessage("Please provide a name.");
        }
    }
}
