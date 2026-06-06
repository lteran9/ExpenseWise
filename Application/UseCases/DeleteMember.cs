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
                var response = await _repository.RemoveMemberAsync(new MemberOf() { Id = request.Id });
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
        public int Id { get; set; }
    }

    public class DeleteMemberResponse
    {
        public bool Success { get; set; }
    }
}
