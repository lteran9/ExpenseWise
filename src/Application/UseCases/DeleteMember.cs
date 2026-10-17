using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class DeleteMember : BaseRequestHandler<DeleteMemberRequest, DeleteMemberResponse>
    {
        private readonly IGroupRepository _repository;
        private readonly AbstractValidator<DeleteMemberRequest> _validator;

        public DeleteMember(IGroupRepository repository)
        {
            _repository = repository;
            _validator = new DeleteMemberRequestValidator();
        }

        public override async Task<ResponseWrapper<DeleteMemberResponse>> Handle(DeleteMemberRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var group = await _repository.FindByUniqueKeyAsync(request.GroupKey);
                if (group == null)
                {
                    return Invalid("Unable to find a group for the given key.");
                }

                if (group.Owner.UniqueKey != request.ActorKey)
                {
                    return Invalid("Only the group organizer can remove group members.");
                }

                var member = group.Members.FirstOrDefault(user => user.UniqueKey == request.MemberKey);
                if (member == null)
                {
                    return Invalid("Member not found in the group.");
                }

                var response = await _repository.RemoveMemberAsync(new MemberOf
                {
                    Group = group,
                    User = member
                });
                if (response != null)
                {
                    return Successful(
                        new DeleteMemberResponse()
                        {
                            Success = true
                        });
                }
                else
                {
                    // Member was not found or could not be deleted
                    return Failed(default);
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public class DeleteMemberRequest : IRequest<ResponseWrapper<DeleteMemberResponse>>
    {
        public Guid GroupKey { get; set; }
        public Guid MemberKey { get; set; }
        public Guid ActorKey { get; set; }
    }

    public class DeleteMemberResponse
    {
        public bool Success { get; set; }
    }
}
