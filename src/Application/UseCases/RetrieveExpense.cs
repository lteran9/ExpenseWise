using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using MediatR;

namespace Application.UseCases
{
    public class RetrieveExpense : BaseRequestHandler<RetrieveExpenseRequest, RetrieveExpenseResponse>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IGroupRepository _groupRepository;

        public RetrieveExpense(IExpenseRepository expenseRepository, IGroupRepository groupRepository)
        {
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
        }

        public override async Task<ResponseWrapper<RetrieveExpenseResponse>> Handle(
            RetrieveExpenseRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ActorKey == Guid.Empty || request.ExpenseKey == Guid.Empty)
            {
                return Invalid("An authenticated user and expense key are required.");
            }

            var expense = await _expenseRepository.FindByUniqueKeyAsync(request.ExpenseKey, cancellationToken);
            if (expense?.BelongsTo == null)
            {
                return Invalid("Expense not found.");
            }

            var group = await _groupRepository.FindByUniqueKeyAsync(expense.BelongsTo.UniqueKey);
            if (group == null || !group.Members.Any(member => member.UniqueKey == request.ActorKey))
            {
                return Invalid("Expense not found.");
            }

            return Successful(new RetrieveExpenseResponse { Expense = expense });
        }
    }

    public sealed class RetrieveExpenseRequest : IRequest<ResponseWrapper<RetrieveExpenseResponse>>
    {
        public Guid ActorKey { get; set; }
        public Guid ExpenseKey { get; set; }
    }

    public sealed class RetrieveExpenseResponse
    {
        public Expense Expense { get; set; } = new();
    }
}
