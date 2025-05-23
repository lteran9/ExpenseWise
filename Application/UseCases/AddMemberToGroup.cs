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
      private readonly IDatabasePort<User> _userRepository;
      private readonly IDatabasePort<Group> _groupRepository;
      private readonly IDatabasePort<MemberOf> _memberOfRepository;
      private readonly AbstractValidator<AddMemberToGroupRequest> _validator;

      public AddMemberToGroup(IDatabasePort<MemberOf> repository, IDatabasePort<User> userRepository, IDatabasePort<Group> groupRepository)
      {
         _userRepository = userRepository;
         _groupRepository = groupRepository;
         _memberOfRepository = repository;
         _validator = new AddMemberRequestValidator();
      }

      public override async Task<ResponseWrapper<AddMemberToGroupResponse>> Handle(AddMemberToGroupRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            // Get user
            var user = await _userRepository.RetrieveAsync(new User() { Phone = request.Phone });

            // Get group
            var group = await _groupRepository.RetrieveAsync(new Group() { UniqueKey = request.GroupUniqueKey });

            if (user != null && group != null)
            {
               var membership =
                  new MemberOf()
                  {
                     User = user,
                     Group = group
                  };

               var response = await _memberOfRepository.CreateAsync(membership);
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
            else
            {
               return Invalid("Unable to add user to group.");
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class AddMemberToGroupRequest : IRequest<ResponseWrapper<AddMemberToGroupResponse>>
   {
      public string Phone { get; set; }

      public Guid GroupUniqueKey { get; set; }

      public AddMemberToGroupRequest()
      {
         Phone = string.Empty;
      }
   }

   public class AddMemberToGroupResponse
   {
      public bool Success { get; set; }
   }
}