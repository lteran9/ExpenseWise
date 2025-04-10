using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class DeleteMember : BaseRequestHandler<DeleteMemberRequest, DeleteMemberResponse>
   {
      private readonly IDatabasePort<MemberOf> _repository;
      private readonly AbstractValidator<DeleteMemberRequest> _validator;

      public DeleteMember(IDatabasePort<MemberOf> repository)
      {
         _repository = repository;
         _validator = new DeleteMemberRequestValidator();
      }

      public override async Task<ResponseWrapper<DeleteMemberResponse>> Handle(DeleteMemberRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var membership = await _repository.RetrieveAsync(new MemberOf() { Id = request.Id });
            if (membership != null)
            {
               var response = await _repository.DeleteAsync(membership);
               if (response != null)
               {
                  return Successful(
                     new DeleteMemberResponse()
                     {
                        Success = true
                     });
               }
               else
               {
                  // Member was not found or could not be deleted 
                  return Failed(default);
               }
            }
            else
            {
               return Invalid("Unable to find membership with given id.");
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class DeleteMemberRequest : IRequest<ResponseWrapper<DeleteMemberResponse>>
   {
      public int Id { get; set; }
   }

   public class DeleteMemberResponse
   {
      public bool Success { get; set; }
   }
}