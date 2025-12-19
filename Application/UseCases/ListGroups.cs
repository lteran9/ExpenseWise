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
        private readonly IDatabasePort<User> _userRepository;
        private readonly IQueryPort<Group> _groupQuery;
        private readonly AbstractValidator<ListGroupsRequest> _validator;

        public ListGroups(IQueryPort<Group> query, IDatabasePort<User> repository)
        {
            _groupQuery = query;
            _userRepository = repository;
            _validator = new ListGroupsRequestValidator();
        }

        public override async Task<ResponseWrapper<ListGroupsResponse>> Handle(ListGroupsRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var user = await _userRepository.RetrieveAsync(new User() { UniqueKey = request.UserId });
                if (user != null)
                {
                    var groups = await _groupQuery.FindAsync(new Group() { Owner = user });
                    if (groups?.Any() == true)
                    {
                        return Successful(
                           new ListGroupsResponse()
                           {
                               Groups = groups
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
        public List<Group> Groups { get; set; }

        public ListGroupsResponse()
        {
            Groups = new List<Group>();
        }
    }
}
