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
      private readonly ISqlDatabase<User> _repository;
      private readonly AbstractValidator<DeleteUserRequest> _validator;

      public DeleteUser(ISqlDatabase<User> repository)
      {
         _repository = repository;
         _validator = new DeleteUserRequestValidator();
      }

      public override async Task<ResponseWrapper<DeleteUserResponse>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            // Let repository implementation handle Find operation
            var response = await _repository.Delete(new User() { Id = request.Id });
            if (response != null)
            {
               return Successful(
                  new DeleteUserResponse()
                  {
                     Success = true
                  });
            }
            else
            {
               // User was not found or could not be deleted 
               return Failed(default);
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