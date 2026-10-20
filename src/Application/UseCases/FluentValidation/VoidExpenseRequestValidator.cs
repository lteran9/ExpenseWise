using FluentValidation;

namespace Application.UseCases.FluentValidation
{
    internal sealed class VoidExpenseRequestValidator : AbstractValidator<VoidExpenseRequest>
    {
        public VoidExpenseRequestValidator()
        {
            RuleFor(request => request.ActorKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An authenticated user is required.");
            RuleFor(request => request.GroupKey)
                .NotEqual(Guid.Empty)
                .WithMessage("A group key is required.");
            RuleFor(request => request.ExpenseKey)
                .NotEqual(Guid.Empty)
                .WithMessage("An expense key is required.");
        }
    }
}
