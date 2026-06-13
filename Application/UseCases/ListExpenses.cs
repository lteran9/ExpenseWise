using System;
using Core.Entities;
using Application.UseCases.MediatR;
using MediatR;
using Application.UseCases.Ports;
using Application.UseCases.FluentValidation;

namespace Application.UseCases
{
    public class ListExpenses : BaseRequestHandler<ListExpensesRequest, ListExpensesResponse>
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ListExpensesRequestValidator _validator;

        public ListExpenses(IGroupRepository groupRepository, IExpenseRepository expenseRepository)
        {
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
            _validator = new ListExpensesRequestValidator();
        }

        public override async Task<ResponseWrapper<ListExpensesResponse>> Handle(ListExpensesRequest request, CancellationToken token)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
                if (group != null)
                {
                    var expenses = await _expenseRepository.GetGroupExpenses(group.Id);
                    if (expenses?.Any() == true)
                    {
                        return Successful(
                            new ListExpensesResponse()
                            {
                                GroupKey = request.GroupKey,
                                GroupMembers = group.Members.Count,
                                Expenses = expenses
                            });
                    }
                    else
                    {
                        return Invalid("Unable to find expenses for the given group.");
                    }
                }
                else
                {
                    return Invalid("Unable to find a group for the given key.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class ListExpensesRequest : IRequest<ResponseWrapper<ListExpensesResponse>>
    {
        public Guid GroupKey { get; set; }
    }

    public class ListExpensesResponse
    {
        public int GroupMembers { get; set; }

        public Guid GroupKey { get; set; }

        public List<Expense> Expenses { get; set; }

        public ListExpensesResponse()
        {
            Expenses = new List<Expense>();
        }
    }
}