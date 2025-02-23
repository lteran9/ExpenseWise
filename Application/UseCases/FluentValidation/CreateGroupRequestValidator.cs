using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
   {
      public CreateGroupRequestValidator()
      {
         RuleFor(x => x.OwnerId)
            .GreaterThan(0)
            .WithMessage("Please provide a group owner.");
         RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Please provide a name.");
      }
   }
}