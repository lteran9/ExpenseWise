using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class FindUser : BaseRequestHandler<FindUserRequest, FindUserResponse>
    {
        private readonly IDatabasePort<User> _repository;
        private readonly AbstractValidator<FindUserRequest> _validator;

        public FindUser(IDatabasePort<User> repository)
        {
            _repository = repository;
            _validator = new FindUserRequestValidator();
        }

        public override async Task<ResponseWrapper<FindUserResponse>> Handle(FindUserRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                var user =
                   new User()
                   {
                       UniqueKey = request.UniqueKey
                   };

                var response = await _repository.RetrieveAsync(user);
                if (response != null)
                {
                    return Successful(
                       new FindUserResponse()
                       {
                           Id = response.Id,
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

    public class FindUserRequest : IRequest<ResponseWrapper<FindUserResponse>>
    {
        public Guid UniqueKey { get; set; }
    }

    public class FindUserResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public FindUserResponse()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
        }
    }
}
