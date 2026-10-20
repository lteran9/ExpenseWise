using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using MediatR;

namespace Application.UseCases
{
    public class VoidExpense : BaseRequestHandler<VoidExpenseRequest, VoidExpenseResponse>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly VoidExpenseRequestValidator _validator = new();

        public VoidExpense(IExpenseRepository expenseRepository, IGroupRepository groupRepository)
        {
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
        }

        public override async Task<ResponseWrapper<VoidExpenseResponse>> Handle(
            VoidExpenseRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Invalid(validationResult.Errors.Select(error => error.ErrorMessage).ToList());
            }

            var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
            if (group == null)
            {
                return Invalid("Group not found.");
            }

            if (group.Owner.UniqueKey != request.ActorKey)
            {
                return Invalid("Only the group organizer can void expenses.");
            }

            var outcome = await _expenseRepository.VoidAsync(request.ExpenseKey, group.Id, cancellationToken);
            if (outcome.BlockedByRepayments)
            {
                return Invalid("Expenses cannot be voided after repayments have been recorded in this group.");
            }

            if (outcome.Expense == null)
            {
                return Invalid("Expense not found in the requested group.");
            }

            return Successful(new VoidExpenseResponse
            {
                ExpenseKey = outcome.Expense.UniqueKey,
                Voided = outcome.Expense.Voided
            });
        }
    }

    public sealed class VoidExpenseRequest : IRequest<ResponseWrapper<VoidExpenseResponse>>
    {
        public Guid ActorKey { get; set; }
        public Guid GroupKey { get; set; }
        public Guid ExpenseKey { get; set; }
    }

    public sealed class VoidExpenseResponse
    {
        public Guid ExpenseKey { get; set; }
        public bool Voided { get; set; }
    }
}
