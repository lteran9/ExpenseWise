using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class DeleteGroupRequestValidator : AbstractValidator<DeleteGroupRequest>
   {
      public DeleteGroupRequestValidator()
      {
         RuleFor(x => x.GroupId)
            .GreaterThan(0)
            .WithMessage("Please provide a group id.");
         RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Please provide a user id.");
      }
   }
}