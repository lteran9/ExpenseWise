using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class FindUserRequestValidator : AbstractValidator<FindUserRequest>
   {
      public FindUserRequestValidator()
      {
         RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Please provide a user id to query with.");
      }
   }
}