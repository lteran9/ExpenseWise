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
         RuleFor(x => x.CustomShares)
            .Must((request, shares) => !request.UseCustomAmounts || shares is { Count: > 0 })
            .WithMessage("Provide a custom amount for every selected participant.")
            .Must((request, shares) => request.UseCustomAmounts || shares is not { Count: > 0 })
            .WithMessage("Custom amounts can only be provided when custom splitting is selected.");
         RuleFor(x => x)
            .Must(HaveValidCustomShares)
            .WithMessage("Custom amounts must be nonnegative USD amounts with at most two decimal places.");
         RuleFor(x => x)
            .Must(CustomSharesMatchParticipants)
            .WithMessage("Provide exactly one custom amount for each selected participant.");
         RuleFor(x => x)
            .Must(CustomSharesSumToExpenseAmount)
            .WithMessage("Custom amounts must sum exactly to the expense total.");
      }

      private static bool HaveValidCustomShares(CreateExpenseRequest request)
      {
         if (!request.UseCustomAmounts || request.CustomShares is null)
         {
            return true;
         }

         return request.CustomShares.All(share =>
            share is not null
            && share.ParticipantKey != Guid.Empty
            && share.Amount >= 0
            && decimal.Round(share.Amount, 2) == share.Amount
            && share.Amount <= 9999999999999999.99m);
      }

      private static bool CustomSharesMatchParticipants(CreateExpenseRequest request)
      {
         if (!request.UseCustomAmounts)
         {
            return true;
         }

         if (request.CustomShares is not { Count: > 0 } shares || request.ParticipantKeys is null)
         {
            return false;
         }

         return shares.All(share => share is not null)
            && shares.Select(share => share.ParticipantKey).Distinct().Count() == shares.Count
            && shares.Select(share => share.ParticipantKey).OrderBy(key => key)
               .SequenceEqual(request.ParticipantKeys.OrderBy(key => key));
      }

      private static bool CustomSharesSumToExpenseAmount(CreateExpenseRequest request)
      {
         return !request.UseCustomAmounts
            || request.CustomShares is { Count: > 0 } shares
               && shares.All(share => share is not null)
               && shares.Sum(share => share.Amount) == request.Amount;
      }
   }
}
