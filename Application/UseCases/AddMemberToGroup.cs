using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class AddMemberToGroup : BaseRequestHandler<AddMemberToGroupRequest, AddMemberToGroupResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly AbstractValidator<AddMemberToGroupRequest> _validator;

        public AddMemberToGroup(IUserRepository userRepository, IGroupRepository groupRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _validator = new AddMemberRequestValidator();
        }

        public override async Task<ResponseWrapper<AddMemberToGroupResponse>> Handle(AddMemberToGroupRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                // Get user
                var user = await _userRepository.FindByPhoneAsync(request.Phone);

                // Get group
                var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupUniqueKey);

                if (user != null && group != null)
                {
                    // Shallow validation?
                    if (user.CountryCode == request.CountryCode)
                    {
                        var membership =
                           new MemberOf()
                           {
                               User = user,
                               Group = group
                           };

                        var response = await _groupRepository.AddMemberAsync(membership);
                        if (response != null)
                        {
                            return Successful(
                               new AddMemberToGroupResponse()
                               {
                                   Success = true
                               });
                        }
                        else
                        {
                            return Failed(default);
                        }
                    }
                }
                else
                {
                    return Invalid("Unable to add user to group.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class AddMemberToGroupRequest : IRequest<ResponseWrapper<AddMemberToGroupResponse>>
    {
        public string Phone { get; set; }
        public string CountryCode { get; set; }

        public Guid GroupUniqueKey { get; set; }

        public AddMemberToGroupRequest()
        {
            Phone = string.Empty;
            CountryCode = string.Empty;
        }
    }

    public class AddMemberToGroupResponse
    {
        public bool Success { get; set; }
    }
}
