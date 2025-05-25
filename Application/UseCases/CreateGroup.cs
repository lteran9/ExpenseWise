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
      private readonly IDatabasePort<Group> _groupRepository;
      private readonly IDatabasePort<MemberOf> _memberOfRepository;
      private readonly AbstractValidator<CreateGroupRequest> _validator;

      public CreateGroup(IDatabasePort<Group> groupRepository, IDatabasePort<MemberOf> memberOfRepository)
      {
         _groupRepository = groupRepository;
         _memberOfRepository = memberOfRepository;
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
                  StartDate = request.StartDate,
                  EndDate = request.EndDate
               };

            var groupResponse = await _groupRepository.CreateAsync(group);
            if (groupResponse != null)
            {
               var memberOf =
                  new MemberOf()
                  {
                     Group = new Group() { Id = groupResponse.Id },
                     User = new User() { Id = group.Owner.Id }
                  };

               var memberOfResponse = await _memberOfRepository.CreateAsync(memberOf);
               if (memberOfResponse != null)
               {
                  return Successful(
                     new CreateGroupResponse()
                     {
                        Id = groupResponse.Id,
                        UniqueKey = memberOfResponse.Group.UniqueKey
                     });
               }
               else
               {
                  return Failed(default);
               }
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

      public DateTime? StartDate { get; set; }
      public DateTime? EndDate { get; set; }

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