using System;
using Application.UseCases.MediatR;
using MediatR;

namespace Application.UseCases
{
    public class UpdateExpense : BaseRequestHandler<UpdateExpenseRequest, UpdateExpenseResponse>
    {
        public override Task<ResponseWrapper<UpdateExpenseResponse>> Handle(UpdateExpenseRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Invalid("Expense editing is not available yet."));
        }
    }

    public class UpdateExpenseRequest : IRequest<ResponseWrapper<UpdateExpenseResponse>>
    {

    }

    public class UpdateExpenseResponse
    {

    }
}
