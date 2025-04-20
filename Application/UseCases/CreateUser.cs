using System;
using System.Security.Cryptography;
using System.Text;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class CreateUser : BaseRequestHandler<CreateUserRequest, CreateUserResponse>
   {
      private readonly IDatabasePort<User> _userRepository;
      private readonly IDatabasePort<Password> _passwordRepository;
      private readonly AbstractValidator<CreateUserRequest> _validator;

      public CreateUser(IDatabasePort<User> userRepository, IDatabasePort<Password> passwordRepository)
      {
         _userRepository = userRepository;
         _passwordRepository = passwordRepository;
         _validator = new CreateUserRequestValidator();
      }

      public override async Task<ResponseWrapper<CreateUserResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            // Step 1: Create Password Ciper and Hash
            var encrypted = HashPassword(request.Password, out byte[] cipher);

            // Step 2: Create User and Password entities
            var user =
               new User()
               {
                  Name = request.Name,
                  Email = request.Email,
                  Phone = request.Phone
               };

            var userResponse = await _userRepository.CreateAsync(user);
            if (userResponse != null)
            {
               var password =
                  new Password()
                  {
                     UserId = user.Id,
                     Cipher = Convert.ToHexString(cipher),
                     Encrypted = encrypted
                  };

               var passwordResponse = await _passwordRepository.CreateAsync(password);
               if (passwordResponse != null)
               {
                  return Successful(
                     new CreateUserResponse()
                     {
                        Id = userResponse.Id,
                        UniqueKey = userResponse.UniqueKey
                     });
               }
               else
               {
                  // Delete user since we were unable to create password 
                  await _userRepository.DeleteAsync(user);
               }
            }
            else
            {
               return Failed(default);
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }

      private string HashPassword(string password, out byte[] cipher)
      {
         int keySize = 128;
         int iterations = 350000;
         var hashAlgorithm = HashAlgorithmName.SHA512;
         cipher = RandomNumberGenerator.GetBytes(keySize);

         var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            cipher,
            iterations,
            hashAlgorithm,
            keySize
         );

         return Convert.ToHexString(hash);
      }
   }

   public class CreateUserRequest : IRequest<ResponseWrapper<CreateUserResponse>>
   {
      public string Name { get; set; }
      public string Email { get; set; }
      public string Phone { get; set; }
      public string Password { get; set; }
      public string ConfirmPassword { get; set; }

      public CreateUserRequest()
      {
         Name = string.Empty;
         Email = string.Empty;
         Phone = string.Empty;
         Password = string.Empty;
         ConfirmPassword = string.Empty;
      }
   }

   public class CreateUserResponse
   {
      public int Id { get; set; }
      public Guid UniqueKey { get; set; }
   }
}