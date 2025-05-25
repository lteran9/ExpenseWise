using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using MediatR;

namespace Application.UseCases
{
   public class RetrieveGroup : BaseRequestHandler<RetrieveGroupRequest, RetrieveGroupResponse>
   {
      private readonly IDatabasePort<Group> _repository;
      private readonly RetrieveGroupRequestValidator _validator;

      public RetrieveGroup(IDatabasePort<Group> repository)
      {
         _repository = repository;
         _validator = new RetrieveGroupRequestValidator();
      }

      public override async Task<ResponseWrapper<RetrieveGroupResponse>> Handle(RetrieveGroupRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var group =
               new Group()
               {
                  UniqueKey = request.UniqueKey
               };

            var response = await _repository.RetrieveAsync(group);
            if (response != null)
            {
               return Successful(
                  new RetrieveGroupResponse()
                  {
                     Group = response
                  });
            }
            else
            {
               return Invalid("Unable to find a group with the given key.");
            }
         }

         return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
      }
   }

   public class RetrieveGroupRequest : IRequest<ResponseWrapper<RetrieveGroupResponse>>
   {
      public Guid UniqueKey { get; set; }
   }

   public class RetrieveGroupResponse
   {
      public Group? Group { get; set; }
   }
}