using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class AddMemberToGroup : BaseRequestHandler<AddMemberToGroupRequest, AddMemberToGroupResponse>
   {
      private readonly IDatabasePort<MemberOf> _repository;
      private readonly AbstractValidator<AddMemberToGroupRequest> _validator;

      public AddMemberToGroup(IDatabasePort<MemberOf> repository)
      {
         _repository = repository;
         _validator = new AddMemberRequestValidator();
      }

      public override async Task<ResponseWrapper<AddMemberToGroupResponse>> Handle(AddMemberToGroupRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var membership =
               new MemberOf()
               {
                  User = request.User!,
                  Group = request.Group!
               };

            var response = await _repository.CreateAsync(membership);
            if (response != null)
            {
               return Successful(
                  new AddMemberToGroupResponse()
                  {
                     Success = true
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

   public class AddMemberToGroupRequest : IRequest<ResponseWrapper<AddMemberToGroupResponse>>
   {
      public User? User { get; set; }
      public Group? Group { get; set; }
   }

   public class AddMemberToGroupResponse
   {
      public bool Success { get; set; }
   }
}