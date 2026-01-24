using System;
using Application.UseCases.MediatR;
using MediatR;

namespace Application.UseCases
{
    public class PayExpense : BaseRequestHandler<PayExpenseRequest, PayExpenseResponse>
    {
        public override Task<ResponseWrapper<PayExpenseResponse>> Handle(PayExpenseRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class PayExpenseRequest : IRequest<ResponseWrapper<PayExpenseResponse>>
    {

    }

    public class PayExpenseResponse
    {

    }
}
