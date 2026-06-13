using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class CreateExpense : BaseRequestHandler<CreateExpenseRequest, CreateExpenseResponse>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly AbstractValidator<CreateExpenseRequest> _validator;

        public CreateExpense(IExpenseRepository expenseRepository, IUserRepository userRepository, IGroupRepository groupRepository)
        {
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _validator = new CreateExpenseRequestValidator();
        }

        public override async Task<ResponseWrapper<CreateExpenseResponse>> Handle(CreateExpenseRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var expense =
                    new Expense()
                    {
                        Description = request.Description,
                        Currency = request.Currency,
                        Amount = request.Amount
                    };

                var expenseResponse = await _expenseRepository.CreateAsync(expense);
                if (expenseResponse != null)
                {
                    return Successful(
                        new CreateExpenseResponse()
                        {
                            Id = expenseResponse.Id
                        });
                }

                return Failed(default);
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class CreateExpenseRequest : IRequest<ResponseWrapper<CreateExpenseResponse>>
    {
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }

        public Guid UserKey { get; set; }
        public Guid GroupKey { get; set; }

        public CreateExpenseRequest()
        {
            Description = string.Empty;
            Currency = string.Empty;
        }
    }

    public class CreateExpenseResponse
    {
        public int Id { get; set; }
    }
}
