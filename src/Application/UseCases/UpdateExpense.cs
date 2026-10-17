using System;
using Application.UseCases.MediatR;
using MediatR;

namespace Application.UseCases
{
    public class UpdateExpense : BaseRequestHandler<UpdateExpenseRequest, UpdateExpenseResponse>
    {
        public override Task<ResponseWrapper<UpdateExpenseResponse>> Handle(UpdateExpenseRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class UpdateExpenseRequest : IRequest<ResponseWrapper<UpdateExpenseResponse>>
    {

    }

    public class UpdateExpenseResponse
    {

    }
}
