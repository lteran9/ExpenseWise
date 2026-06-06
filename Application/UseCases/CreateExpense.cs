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
                    // Retrieve existing user and group records by their unique keys so we can reference them by Id
                    var userResponse = await _userRepository.FindByUniqueKeyAsync(request.UserKey);
                    if (userResponse == null)
                    {
                        return Failed(default);
                    }

                    var groupResponse = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
                    if (groupResponse == null)
                    {
                        return Failed(default);
                    }

                    var split =
                        new Split()
                        {
                            User = new User { Id = userResponse.Id },
                            Expense = new Expense { Id = expenseResponse.Id },
                            Group = new Group { Id = groupResponse.Id },
                        };

                    var splitResponse = await _expenseRepository.AddSplitAsync(split);
                    if (splitResponse != null)
                    {
                        return Successful(
                            new CreateExpenseResponse()
                            {
                                Id = expenseResponse.Id
                            });
                    }
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
