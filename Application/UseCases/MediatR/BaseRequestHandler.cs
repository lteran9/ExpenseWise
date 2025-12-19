using System;
using FluentValidation;
using MediatR;

namespace Application.UseCases.MediatR
{
    public abstract class BaseRequestHandler<TRequest, TResponse>
       : IRequestHandler<TRequest, ResponseWrapper<TResponse>>
       where TRequest : IRequest<ResponseWrapper<TResponse>>
    {
        // IRequestHandler
        public abstract Task<ResponseWrapper<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);

        protected ResponseWrapper<TResponse> Failed(TResponse? obj, Exception? ex = null)
        {
            return
               new ResponseWrapper<TResponse>()
               {
                   Succeeded = false,
                   Result = obj ?? default,
                   Error = ex?.Message ?? ""
               };
        }

        protected ResponseWrapper<TResponse> Invalid(string message)
        {
            return
               new ResponseWrapper<TResponse>()
               {
                   Succeeded = false,
                   ValidationMessages = new List<string>() { message ?? "" }
               };
        }

        protected ResponseWrapper<TResponse> Invalid(List<string> messages)
        {
            return
               new ResponseWrapper<TResponse>()
               {
                   Succeeded = false,
                   ValidationMessages = messages
               };
        }

        protected ResponseWrapper<TResponse> Successful(TResponse? result)
        {
            return
               new ResponseWrapper<TResponse>()
               {
                   Succeeded = true,
                   Result = result
               };
        }
    }
}
