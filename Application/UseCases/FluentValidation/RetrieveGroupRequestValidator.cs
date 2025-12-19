using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    public class RetrieveGroupRequestValidator : AbstractValidator<RetrieveGroupRequest>
    {
        public RetrieveGroupRequestValidator()
        {
            RuleFor(x => x.UniqueKey)
               .NotEqual(Guid.Empty)
               .WithMessage("Please provide a key to query for.");
        }
    }
}
