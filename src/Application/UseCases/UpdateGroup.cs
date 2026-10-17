using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public sealed class UpdateGroup : BaseRequestHandler<UpdateGroupRequest, UpdateGroupResponse>
    {
        private readonly IGroupRepository _repository;
        private readonly AbstractValidator<UpdateGroupRequest> _validator;

        public UpdateGroup(IGroupRepository repository)
        {
            _repository = repository;
            _validator = new UpdateGroupRequestValidator();
        }

        public override async Task<ResponseWrapper<UpdateGroupResponse>> Handle(UpdateGroupRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var existingGroup = await _repository.FindByUniqueKeyAsync(request.UniqueKey);
                if (existingGroup == null)
                {
                    return Invalid("Group not found.");
                }

                existingGroup.Name = request.Name;
                existingGroup.Active = request.Active;
                existingGroup.StartDate = request.StartDate;
                existingGroup.EndDate = request.EndDate;

                var response = await _repository.UpdateAsync(existingGroup);
                if (response != null)
                {
                    return Successful(
                        new UpdateGroupResponse()
                        {
                            Success = true
                        });
                }
                else
                {
                    return Invalid("Failed to update group.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record UpdateGroupRequest : IRequest<ResponseWrapper<UpdateGroupResponse>>
    {
        public bool Active { get; set; }

        public string Name { get; set; }

        public Guid UniqueKey { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public UpdateGroupRequest()
        {
            Name = string.Empty;
        }
    }

    public record UpdateGroupResponse
    {
        public bool Success { get; set; }
    }
}
