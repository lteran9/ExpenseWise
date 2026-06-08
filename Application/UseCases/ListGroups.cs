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
        private readonly IExpenseRepository _expenseRepository;
        private readonly IQueryPort<Group> _groupQuery;
        private readonly AbstractValidator<ListGroupsRequest> _validator;

        public ListGroups(IQueryPort<Group> query, IUserRepository repository, IExpenseRepository expenseRepository)
        {
            _groupQuery = query;
            _userRepository = repository;
            _expenseRepository = expenseRepository;
            _validator = new ListGroupsRequestValidator();
        }

        public override async Task<ResponseWrapper<ListGroupsResponse>> Handle(ListGroupsRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var user = await _userRepository.FindByUniqueKeyAsync(request.UserId);
                if (user != null)
                {
                    var groups = await _groupQuery.FindAsync(new Group() { Owner = user });
                    if (groups?.Any() == true)
                    {
                        var response = new List<RetrieveGroupResponse>();

                        // Get expense list
                        foreach (var group in groups)
                        {
                            var expenseList = await _expenseRepository.GetGroupExpenses(group.Id);
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
                                    Expensed = expenseList?.Sum(x => x.Amount) ?? 0,
                                    Outstanding = expenseList?.Where(e => e.Settled == false).Sum(x => x.Amount) ?? 0
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
                else
                {
                    return Invalid("Could not find a user associated with the requested id.");
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
