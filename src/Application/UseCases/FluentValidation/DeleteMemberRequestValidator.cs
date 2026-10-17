using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class DeleteMemberRequestValidator : AbstractValidator<DeleteMemberRequest>
    {
        public DeleteMemberRequestValidator()
        {
            RuleFor(x => x.GroupKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a group key.");
            RuleFor(x => x.MemberKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a member key.");
            RuleFor(x => x.ActorKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An authenticated user is required.");
        }
    }
}
