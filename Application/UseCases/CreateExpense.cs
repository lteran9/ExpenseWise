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
        private readonly IDatabasePort<Expense> _repository;
        private readonly IDatabasePort<Split> _splitRepository;
        private readonly IDatabasePort<User> _userRepository;
        private readonly IDatabasePort<Group> _groupRepository;
        private readonly AbstractValidator<CreateExpenseRequest> _validator;

        public CreateExpense(IDatabasePort<Expense> repository, IDatabasePort<Split> splitRepository, IDatabasePort<User> userRepository, IDatabasePort<Group> groupRepository)
        {
            _repository = repository;
            _splitRepository = splitRepository;
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

                var expenseResponse = await _repository.CreateAsync(expense);
                if (expenseResponse != null)
                {
                    // Retrieve existing user and group records by their unique keys so we can reference them by Id
                    var userResponse = await _userRepository.RetrieveAsync(new User { UniqueKey = request.UserKey });
                    if (userResponse == null)
                    {
                        return Failed(default);
                    }

                    var groupResponse = await _groupRepository.RetrieveAsync(new Group { UniqueKey = request.GroupKey });
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

                    var splitResponse = await _splitRepository.CreateAsync(split);
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
