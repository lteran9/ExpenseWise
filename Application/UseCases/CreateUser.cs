using System;
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
      private readonly ISqlDatabase<User> _repository;
      private readonly AbstractValidator<CreateUserRequest> _validator;

      public CreateUser(ISqlDatabase<User> repository)
      {
         _repository = repository;
         _validator = new CreateUserRequestValidator();
      }

      public override async Task<ResponseWrapper<CreateUserResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var user =
               new User()
               {
                  Name = request.Name,
                  Email = request.Email,
                  Phone = request.Phone
               };

            var response = await _repository.CreateAsync(user);
            if (response != null)
            {
               return Successful(
                  new CreateUserResponse()
                  {
                     Id = response.Id,
                     UniqueKey = response.UniqueKey
                  });
            }
            else
            {
               return Failed(default);
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class CreateUserRequest : IRequest<ResponseWrapper<CreateUserResponse>>
   {
      public string Name { get; set; }
      public string Email { get; set; }
      public string Phone { get; set; }
      public string Password { get; set; }

      public CreateUserRequest()
      {
         Name = string.Empty;
         Email = string.Empty;
         Phone = string.Empty;
         Password = string.Empty;
      }
   }

   public class CreateUserResponse
   {
      public int Id { get; set; }
      public Guid UniqueKey { get; set; }
   }
}