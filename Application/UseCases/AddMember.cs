using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class AddMember : BaseRequestHandler<AddMemberRequest, AddMemberResponse>
   {
      private readonly ISqlDatabase<MemberOf> _repository;
      private readonly AbstractValidator<AddMemberRequest> _validator;

      public AddMember(ISqlDatabase<MemberOf> repository)
      {
         _repository = repository;
         _validator = new AddMemberRequestValidator();
      }

      public override async Task<ResponseWrapper<AddMemberResponse>> Handle(AddMemberRequest request, CancellationToken cancellationToken)
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
                  new AddMemberResponse()
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

   public class AddMemberRequest : IRequest<ResponseWrapper<AddMemberResponse>>
   {
      public User? User { get; set; }
      public Group? Group { get; set; }
   }

   public class AddMemberResponse
   {
      public bool Success { get; set; }
   }
}