using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class FindUserRequestValidator : AbstractValidator<FindUserRequest>
   {
      public FindUserRequestValidator()
      {
         RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Please provide a user id to query with.");
      }
   }
}