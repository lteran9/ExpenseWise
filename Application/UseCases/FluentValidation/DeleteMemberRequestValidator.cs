using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal class DeleteMemberRequestValidator : AbstractValidator<DeleteMemberRequest>
    {
        public DeleteMemberRequestValidator()
        {
            RuleFor(x => x.Id)
               .GreaterThan(0)
               .WithMessage("Please provide a user id.");
        }
    }
}
