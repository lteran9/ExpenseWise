using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
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
               .WithMessage("Please provide an email address.")
               .EmailAddress()
               .WithMessage("Please provide a valid email address.");
            RuleFor(x => x.Phone)
               .NotEmpty()
               .WithMessage("Please provide a phone number for the account.")
               .Matches(new Regex(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}"))
               .WithMessage("Please enter a valid phone number.");
            RuleFor(x => x.Password)
               .NotEmpty()
               .WithMessage("Please provide a password for the account.")
               .Must(MatchPasswordRegexRules)
               .WithMessage("Password must be between 8 and 25 characters long with at least one special character.");
            RuleFor(x => x)
               .Must(PasswordMatchConfirmPassword)
               .WithMessage("Password and Confirm Password do not match.");
        }

        private bool PasswordMatchConfirmPassword(CreateUserRequest request)
        {
            return request.Password.Equals(request.ConfirmPassword);
        }

        private bool MatchPasswordRegexRules(string password)
        {
            return Regex.Match(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,25}$").Success;
        }
    }
}