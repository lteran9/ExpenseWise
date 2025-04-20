using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
   {
      public AuthenticateUserRequestValidator()
      {
         RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Please provide an email address.")
            .EmailAddress()
            .WithMessage("Please provide a valid email address.");

         RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Please provide a password.");
      }
   }
}