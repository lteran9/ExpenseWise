using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class DeleteGroupRequestValidator : AbstractValidator<DeleteGroupRequest>
    {
        public DeleteGroupRequestValidator()
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
