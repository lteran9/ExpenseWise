using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class CreateGroup : BaseRequestHandler<CreateGroupRequest, CreateGroupResponse>
    {
        private readonly IGroupRepository _groupRepository;
        private readonly AbstractValidator<CreateGroupRequest> _validator;

        public CreateGroup(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            _validator = new CreateGroupRequestValidator();
        }

        public override async Task<ResponseWrapper<CreateGroupResponse>> Handle(CreateGroupRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var group = new Group
                {
                    Owner = new User { UniqueKey = request.ActorKey },
                    Name = request.Name,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                var groupResponse = await _groupRepository.CreateWithOwnerMembershipAsync(group);
                if (groupResponse == null)
                {
                    return Invalid("Unable to create group for the authenticated user.");
                }

                return Successful(new CreateGroupResponse
                {
                    Id = groupResponse.Id,
                    UniqueKey = groupResponse.UniqueKey
                });
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class CreateGroupRequest : IRequest<ResponseWrapper<CreateGroupResponse>>
    {
        public Guid ActorKey { get; set; }

        public string Name { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public CreateGroupRequest()
        {
            Name = string.Empty;
        }
    }

    public class CreateGroupResponse
    {
        public int Id { get; set; }
        public Guid UniqueKey { get; set; }
    }
}
