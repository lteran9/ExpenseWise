using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.UniqueKey)
                .NotEqual(Guid.Empty)
                .WithMessage("Please provide a user id to query with.");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Please provide a name.");
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Please provide an email address.")
                .EmailAddress()
                .WithMessage("Please provide a valid email address.");
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Please provide a phone number for the account.")
                .Matches(new Regex(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}"))
                .WithMessage("Please enter a valid phone number.");
        }
    }
}
