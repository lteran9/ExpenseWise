using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using MediatR;

namespace Application.UseCases
{
    public class RetrieveGroup : BaseRequestHandler<RetrieveGroupRequest, RetrieveGroupResponse>
    {
        private readonly IDatabasePort<Group> _repository;
        private readonly RetrieveGroupRequestValidator _validator;

        public RetrieveGroup(IDatabasePort<Group> repository)
        {
            _repository = repository;
            _validator = new RetrieveGroupRequestValidator();
        }

        public override async Task<ResponseWrapper<RetrieveGroupResponse>> Handle(RetrieveGroupRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var group =
                   new Group()
                   {
                       UniqueKey = request.UniqueKey
                   };

                var response = await _repository.RetrieveAsync(group);
                if (response != null)
                {
                    return Successful(
                       new RetrieveGroupResponse()
                       {
                           Active = response.Active,
                           Name = response.Name,
                           StartDate = response.StartDate ?? DateTime.MinValue,
                           EndDate = response.EndDate ?? DateTime.MinValue,
                           OwnerId = response.Owner.UniqueKey,
                           Members = response.Members.Select(x => new FindUserResponse() { UniqueKey = x.UniqueKey, Name = x.Name, Email = x.Email, Phone = x.Phone }).ToList()
                       });
                }
                else
                {
                    return Invalid("Unable to find a group with the given key.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record RetrieveGroupRequest : IRequest<ResponseWrapper<RetrieveGroupResponse>>
    {
        public Guid UniqueKey { get; set; }
    }

    public record RetrieveGroupResponse
    {
        public bool Active { get; set; }

        public string Name { get; set; }

        public Guid OwnerId { get; set; }
        public Guid UniqueKey { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<FindUserResponse> Members { get; set; }

        public RetrieveGroupResponse()
        {
            Name = string.Empty;
            Members = new List<FindUserResponse>();
        }
    }
}
