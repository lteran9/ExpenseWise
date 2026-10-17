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
                var groups = await _groupRepository.ListAsync(request.UserId);
                if (groups?.Any() == true)
                {
                    var response = new List<RetrieveGroupResponse>();

                    // Get expense list
                    foreach (var group in groups)
                    {
                        var expenseList = await _expenseRepository.GetGroupExpenses(group.Id);
                        var paymentsList = await _expenseRepository.GetSplitsAsync(group.Id);

                        var expensed = expenseList?.Sum(x => x.Amount) ?? 0;
                        var outstanding = expensed - (paymentsList?.Where(e => e.Paid == true).Sum(x => x.Amount) ?? 0);
                        response.Add(
                            new RetrieveGroupResponse()
                            {
                                Active = group.Active,
                                Name = group.Name,
                                StartDate = group.StartDate ?? DateTime.MinValue,
                                EndDate = group.EndDate ?? DateTime.MinValue,
                                OwnerId = group.Owner.UniqueKey,
                                UniqueKey = group.UniqueKey,
                                Members = group.Members.Select(y => new FindUserResponse() { Name = y.Name, Email = y.Email, Phone = y.Phone, UniqueKey = y.UniqueKey }).ToList(),
                                Expensed = expensed > 0 ? expensed : 0,
                                Outstanding = outstanding > 0 ? outstanding : 0
                            }
                        );
                    }

                    return Successful(
                        new ListGroupsResponse()
                        {
                            Groups = response
                        });
                }
                else
                {
                    return Invalid("User does not own any groups.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record ListGroupsRequest : IRequest<ResponseWrapper<ListGroupsResponse>>
    {
        public Guid UserId { get; set; }
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
