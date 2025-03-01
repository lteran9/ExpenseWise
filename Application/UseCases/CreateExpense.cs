using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
   public class CreateExpense : BaseRequestHandler<CreateExpenseRequest, CreateExpenseResponse>
   {
      private readonly ISqlDatabase<Expense> _repository;
      private readonly AbstractValidator<CreateExpenseRequest> _validator;

      public CreateExpense(ISqlDatabase<Expense> repository)
      {
         _repository = repository;
         _validator = new CreateExpenseRequestValidator();
      }

      public override async Task<ResponseWrapper<CreateExpenseResponse>> Handle(CreateExpenseRequest request, CancellationToken cancellationToken)
      {
         var validationResult = await _validator.ValidateAsync(request);
         if (validationResult.IsValid)
         {
            var expense =
               new Expense()
               {
                  Description = request.Description,
                  Currency = request.Currency,
                  Amount = request.Amount
               };

            var response = await _repository.Create(expense);
            if (response != null)
            {
               return Successful(
                  new CreateExpenseResponse()
                  {
                     Id = response.Id
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

   public class CreateExpenseRequest : IRequest<ResponseWrapper<CreateExpenseResponse>>
   {
      public string Description { get; set; }
      public string Currency { get; set; }
      public decimal Amount { get; set; }

      public CreateExpenseRequest()
      {
         Description = string.Empty;
         Currency = string.Empty;
      }
   }

   public class CreateExpenseResponse
   {
      public int Id { get; set; }
   }
}