using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class AddMemberRequestValidator : AbstractValidator<AddMemberRequest>
   {
      public AddMemberRequestValidator()
      {
         RuleFor(x => x.User)
            .NotNull()
            .WithMessage("Please include a user object.");
         RuleFor(x => x.Group)
            .NotNull()
            .WithMessage("Please include a group object.");

         When(x => x.User != null, () =>
         {
            RuleFor(x => x.User!.Id)
               .GreaterThan(0)
               .WithMessage("Please provide a valid user.");
         });
         When(x => x.Group != null, () =>
         {
            RuleFor(x => x.Group!.Id)
               .GreaterThan(0)
               .WithMessage("Please provide a valid group.");
         });
      }
   }
}