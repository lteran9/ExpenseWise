using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class AddMemberRequestValidator : AbstractValidator<AddMemberToGroupRequest>
   {
      public AddMemberRequestValidator()
      {
         RuleFor(x => x.Phone)
            .NotNull()
            .WithMessage("Please provide a phone.")
            .Matches(new Regex(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}"))
            .WithMessage("Please enter a valid phone number.");
         RuleFor(x => x.GroupUniqueKey)
            .NotEqual(Guid.Empty)
            .WithMessage("Please provide a group key.");
      }
   }
}