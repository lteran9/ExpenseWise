using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class AuthenticateUser : BaseRequestHandler<AuthenticateUserRequest, AuthenticateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordRepository _passwordRepository;
        private readonly AbstractValidator<AuthenticateUserRequest> _validator;

        public AuthenticateUser(IUserRepository userRepository, IPasswordRepository passwordRepository)
        {
            _userRepository = userRepository;
            _passwordRepository = passwordRepository;
            _validator = new AuthenticateUserRequestValidator();
        }

        public override async Task<ResponseWrapper<AuthenticateUserResponse>> Handle(AuthenticateUserRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (validationResult.IsValid)
            {
                var user = await _userRepository.FindByEmailAsync(request.Email);

                if (user != null)
                {
                    var password = await _passwordRepository.FindByUserIdAsync(user.Id);

                    if (password != null)
                    {
                        if (password.Encrypted == Password.Hash(request.Password, password.Cipher))
                        {
                            return Successful(
                                new AuthenticateUserResponse()
                                {
                                    Id = user.UniqueKey
                                });
                        }
                    }
                }

                return Invalid("Unable to find user account with given email.");
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }
    }

    public record AuthenticateUserRequest : IRequest<ResponseWrapper<AuthenticateUserResponse>>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public record AuthenticateUserResponse
    {
        public required Guid Id { get; set; }
    }
}
