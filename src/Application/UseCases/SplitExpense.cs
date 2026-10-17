using System;
using MediatR;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Application.UseCases.FluentValidation;
using Core.Entities;

namespace Application.UseCases
{
    public class SplitExpense : BaseRequestHandler<SplitExpenseRequest, SplitExpenseResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly SplitExpenseRequestValidator _validator;

        public SplitExpense(IUserRepository userRepository, IGroupRepository groupRepository, IExpenseRepository expenseRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
            _validator = new SplitExpenseRequestValidator();
        }

        public override async Task<ResponseWrapper<SplitExpenseResponse>> Handle(SplitExpenseRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (validationResult.IsValid)
            {
                var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
                if (group == null)
                {
                    return Failed(default);
                }

                var user = await _userRepository.FindByUniqueKeyAsync(request.UserKey);
                if (user == null)
                {
                    return Failed(default);
                }

                var expenses = await _expenseRepository.GetGroupExpenses(group.Id);
                if (expenses?.Any() == true)
                {
                    foreach (var exp in expenses)
                    {
                        var split =
                            new Split()
                            {
                                Group = group,
                                User = user,
                                Expense = exp,
                                Paid = true,
                                PaidOn = DateTime.Now,
                                Amount = exp.Amount / group.Members.Count
                            };
                        await _expenseRepository.AddSplitAsync(split);
                    }

                    return Successful(new SplitExpenseResponse() { Success = true });
                }
                else
                {
                    return Invalid("Given group does not have any expenses yet.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class SplitExpenseRequest : IRequest<ResponseWrapper<SplitExpenseResponse>>
    {
        public Guid UserKey { get; set; }
        public Guid GroupKey { get; set; }
    }

    public class SplitExpenseResponse
    {
        public bool Success { get; set; }
    }
}
