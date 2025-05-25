using System;
using Application.UseCases.MediatR;
using MediatR;

namespace Application.UseCases
{
   public class SplitExpense : BaseRequestHandler<SplitExpenseRequest, SplitExpenseResponse>
   {
      public override Task<ResponseWrapper<SplitExpenseResponse>> Handle(SplitExpenseRequest request, CancellationToken cancellationToken)
      {
         throw new NotImplementedException();
      }
   }

   public class SplitExpenseRequest : IRequest<ResponseWrapper<SplitExpenseResponse>>
   {

   }

   public class SplitExpenseResponse
   {

   }
}