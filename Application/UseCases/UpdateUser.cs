using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public sealed class UpdateUser : BaseRequestHandler<UpdateUserRequest, UpdateUserResponse>
    {
        private readonly IDatabasePort<User> _repository;
        private readonly AbstractValidator<UpdateUserRequest> _validator;

        public UpdateUser(IDatabasePort<User> repository)
        {
            _repository = repository;
            _validator = new UpdateUserRequestValidator();
        }

        public override async Task<ResponseWrapper<UpdateUserResponse>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var user =
                   new User()
                   {
                       UniqueKey = request.UniqueKey,
                       Name = request.Name,
                       Email = request.Email,
                       Phone = request.Phone,
                   };

                var response = await _repository.UpdateAsync(user);
                if (response != null)
                {
                    return Successful(
                       new UpdateUserResponse()
                       {
                           Name = response.Name,
                           Phone = response.Phone,
                           Email = response.Email,
                       });
                }
                else
                {
                    return Invalid("User id not found.");
                }
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record UpdateUserRequest : IRequest<ResponseWrapper<UpdateUserResponse>>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CountryCode { get; set; }

        public Guid UniqueKey { get; set; }

        public UpdateUserRequest()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            CountryCode = string.Empty;
        }
    }

    public record UpdateUserResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public UpdateUserResponse()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
        }
    }
}
