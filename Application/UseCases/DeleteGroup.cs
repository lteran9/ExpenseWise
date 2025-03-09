using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class DeleteGroup : BaseRequestHandler<DeleteGroupRequest, DeleteGroupResponse>
   {
      private readonly ISqlDatabase<Group> _repository;
      private readonly AbstractValidator<DeleteGroupRequest> _validator;

      public DeleteGroup(ISqlDatabase<Group> repository)
      {
         _repository = repository;
         _validator = new DeleteGroupRequestValidator();
      }

      public override async Task<ResponseWrapper<DeleteGroupResponse>> Handle(DeleteGroupRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var group = await _repository.GetAsync(new Group() { Id = request.GroupId });
            if (group != null && group.Owner.Id == request.RequestingUserId)
            {
               var response = await _repository.DeleteAsync(group);
               if (response != null)
               {
                  return Successful(
                     new DeleteGroupResponse()
                     {
                        Success = true
                     });
               }
               else
               {
                  return Failed(default);
               }
            }
            else
            {
               return Invalid("Only the group owner can request a delete.");
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class DeleteGroupRequest : IRequest<ResponseWrapper<DeleteGroupResponse>>
   {
      public int GroupId { get; set; }
      public int RequestingUserId { get; set; }
   }

   public class DeleteGroupResponse
   {
      public bool Success { get; set; }
   }
}