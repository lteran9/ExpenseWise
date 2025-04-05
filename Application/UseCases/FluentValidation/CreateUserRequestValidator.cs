using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
   {
      public CreateUserRequestValidator()
      {
         RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Please provide a name.");
         RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Please provide an email address.");
         RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Please provide a phone number for the account.");
         RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Please provide a password for the account.");
      }
   }
}