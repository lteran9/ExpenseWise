using System;
using FluentValidation;

namespace Application.UseCases.FluentValidation
{
   internal class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
   {
      public CreateExpenseRequestValidator()
      {
         RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Please provide a description for the expense.");
         RuleFor(x => x.Currency)
            .Equal("USD", StringComparer.OrdinalIgnoreCase)
            .WithMessage("Only USD expenses are supported.");
         RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Expense amount must be greater than zero.");
         RuleFor(x => x.Amount)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Expense amounts may have at most two decimal places.");
         RuleFor(x => x.Amount)
            .LessThanOrEqualTo(9999999999999999.99m)
            .WithMessage("Expense amount exceeds the supported range.");
         RuleFor(x => x.GroupKey)
            .NotEqual(Guid.Empty)
            .WithMessage("Please provide a group key to associate with.");
         RuleFor(x => x.ActorKey)
            .NotEqual(Guid.Empty)
            .WithMessage("An authenticated user is required.");
         RuleFor(x => x.IdempotencyKey)
            .NotEqual(Guid.Empty)
            .WithMessage("An expense request key is required.");
         RuleFor(x => x.PayerKey)
            .NotEqual(Guid.Empty)
            .WithMessage("A payer is required.");
         RuleFor(x => x.ParticipantKeys)
            .Must(keys => keys is { Count: > 0 })
            .WithMessage("Select at least one expense participant.")
            .Must(keys => keys != null && keys.Distinct().Count() == keys.Count)
            .WithMessage("Expense participants must be unique.")
            .Must(keys => keys != null && keys.All(key => key != Guid.Empty))
            .WithMessage("Expense participant keys must be valid.");
      }
   }
}
