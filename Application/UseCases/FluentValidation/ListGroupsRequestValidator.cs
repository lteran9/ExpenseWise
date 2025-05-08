using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class ListGroupsRequestValidator : AbstractValidator<ListGroupsRequest>
   {
      public ListGroupsRequestValidator()
      {
         RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("Please provide a user id to query with.");
      }
   }
}