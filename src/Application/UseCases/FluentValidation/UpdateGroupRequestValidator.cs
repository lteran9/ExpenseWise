using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
    {
        public UpdateGroupRequestValidator()
        {
            RuleFor(x => x.UniqueKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a key to query with.");
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("Please provide a name for the group.")
                .MinimumLength(3)
                .WithMessage("Name must be greater than three characters.");
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date cannot be bigger than end date.");
        }
    }
}
