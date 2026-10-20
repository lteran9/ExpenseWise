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
        private readonly AbstractValidator<ListGroupsRequest> _validator;

        public ListGroups(IUserRepository userRepository, IGroupRepository groupRepository, IExpenseRepository expenseRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _expenseRepository = expenseRepository;
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
                    var expenseList = await _expenseRepository.GetGroupExpenses(group.Id);
                    var activeExpenses = expenseList?.Where(expense => !expense.Voided).ToList() ?? [];
                    var expensed = activeExpenses.Sum(expense => expense.Amount);
                    var balances = new Dictionary<Guid, decimal>();
                    foreach (var expense in activeExpenses)
                    {
                        if (expense.Payer != null)
                        {
                            balances[expense.Payer.UniqueKey] = balances.GetValueOrDefault(expense.Payer.UniqueKey) + expense.Amount;
                        }

                        foreach (var share in expense.Shares)
                        {
                            balances[share.Participant.UniqueKey] =
                                balances.GetValueOrDefault(share.Participant.UniqueKey) - share.Amount;
                        }
                    }

                    var outstanding = balances.Values.Where(balance => balance > 0).Sum();
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
                        Expensed = Math.Max(expensed, 0),
                        Outstanding = Math.Max(outstanding, 0)
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
