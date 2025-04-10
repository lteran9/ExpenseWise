using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class DeleteUser : BaseRequestHandler<DeleteUserRequest, DeleteUserResponse>
   {
      private readonly IDatabasePort<User> _repository;
      private readonly AbstractValidator<DeleteUserRequest> _validator;

      public DeleteUser(IDatabasePort<User> repository)
      {
         _repository = repository;
         _validator = new DeleteUserRequestValidator();
      }

      public override async Task<ResponseWrapper<DeleteUserResponse>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var user = await _repository.RetrieveAsync(new User() { Id = request.Id });
            if (user != null)
            {
               var response = await _repository.DeleteAsync(user);
               if (response != null)
               {
                  return Successful(
                     new DeleteUserResponse()
                     {
                        Success = true
                     });
               }
            }
            else
            {
               return Invalid("User id not found.");
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class DeleteUserRequest : IRequest<ResponseWrapper<DeleteUserResponse>>
   {
      public int Id { get; set; }
   }

   public class DeleteUserResponse
   {
      public bool Success { get; set; }
   }
}