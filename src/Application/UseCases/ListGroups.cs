using System;
using MediatR;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using FluentValidation;
using Core.Entities;

namespace Application.UseCases
{
    public class ListGroups : BaseRequestHandler<ListGroupsRequest, ListGroupsResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IRepaymentRepository _repaymentRepository;
        private readonly AbstractValidator<ListGroupsRequest> _validator;

        public ListGroups(
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IExpenseRepository expenseRepository,
            IRepaymentRepository repaymentRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
            _repaymentRepository = repaymentRepository;
            _validator = new ListGroupsRequestValidator();
        }

        public override async Task<ResponseWrapper<ListGroupsResponse>> Handle(ListGroupsRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var groups = await _groupRepository.ListAsync(request.ActorKey);
                var response = new List<RetrieveGroupResponse>();

                foreach (var group in groups ?? new List<Group>())
                {
                    var expenses = await _expenseRepository.GetGroupExpenses(group.Id) ?? new List<Expense>();
                    var repayments = await _repaymentRepository.GetGroupRepaymentsAsync(group.Id, cancellationToken) ?? new List<Repayment>();
                    var ledger = new GroupLedger(group.Members, expenses, repayments);
                    response.Add(new RetrieveGroupResponse
                    {
                        Active = group.Active,
                        Name = group.Name,
                        StartDate = group.StartDate ?? DateTime.MinValue,
                        EndDate = group.EndDate ?? DateTime.MinValue,
                        OwnerId = group.Owner.UniqueKey,
                        UniqueKey = group.UniqueKey,
                        Members = group.Members.Select(member => new FindUserResponse
                        {
                            Name = member.Name,
                            Email = member.Email,
                            Phone = member.Phone,
                            UniqueKey = member.UniqueKey
                        }).ToList(),
                        Expensed = Math.Max(ledger.TotalSpent, 0),
                        Outstanding = Math.Max(ledger.Outstanding, 0)
                    });
                }

                return Successful(new ListGroupsResponse { Groups = response });
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record ListGroupsRequest : IRequest<ResponseWrapper<ListGroupsResponse>>
    {
        public Guid ActorKey { get; set; }
    }

    public record ListGroupsResponse
    {
        public List<RetrieveGroupResponse> Groups { get; set; }

        public ListGroupsResponse()
        {
            Groups = new List<RetrieveGroupResponse>();
        }
    }
}
