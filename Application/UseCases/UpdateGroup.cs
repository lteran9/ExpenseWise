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
        private readonly IDatabasePort<Group> _repository;
        private readonly AbstractValidator<UpdateGroupRequest> _validator;

        public UpdateGroup(IDatabasePort<Group> repository)
        {
            _repository = repository;
            _validator = new UpdateGroupRequestValidator();
        }

        public override async Task<ResponseWrapper<UpdateGroupResponse>> Handle(UpdateGroupRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var user =
                    new Group()
                    {
                        UniqueKey = request.UniqueKey,
                        Name = request.Name,
                        Active = request.Active,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate
                    };

                var response = await _repository.UpdateAsync(user);
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
                    return Invalid("Group id not found.");
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
