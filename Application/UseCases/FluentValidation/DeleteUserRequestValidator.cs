using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
   {
      public DeleteUserRequestValidator()
      {
         RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Please provide a user id.");
      }
   }
}