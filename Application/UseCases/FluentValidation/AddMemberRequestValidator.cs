using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class AddMemberRequestValidator : AbstractValidator<AddMemberToGroupRequest>
   {
      public AddMemberRequestValidator()
      {
         RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Please provide a name.");
         RuleFor(x => x.Phone)
            .NotNull()
            .WithMessage("Please provide a phone.");
         RuleFor(x => x.GroupUniqueKey)
            .NotEqual(Guid.Empty)
            .WithMessage("Please provide a group key.");
      }
   }
}