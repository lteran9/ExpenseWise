using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class CreateGroup : BaseRequestHandler<CreateGroupRequest, CreateGroupResponse>
   {
      private readonly IDatabasePort<Group> _repository;
      private readonly AbstractValidator<CreateGroupRequest> _validator;

      public CreateGroup(IDatabasePort<Group> repository)
      {
         _repository = repository;
         _validator = new CreateGroupRequestValidator();
      }

      public override async Task<ResponseWrapper<CreateGroupResponse>> Handle(CreateGroupRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var group =
               new Group()
               {
                  Owner = new User() { Id = request.OwnerId },
                  Name = request.Name,
               };

            var response = await _repository.CreateAsync(group);
            if (response != null)
            {
               return Successful(
                  new CreateGroupResponse()
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

   public class CreateGroupRequest : IRequest<ResponseWrapper<CreateGroupResponse>>
   {
      public int OwnerId { get; set; }
      public string Name { get; set; }

      public CreateGroupRequest()
      {
         Name = string.Empty;
      }
   }

   public class CreateGroupResponse
   {
      public int Id { get; set; }
      public Guid UniqueKey { get; set; }
   }
}