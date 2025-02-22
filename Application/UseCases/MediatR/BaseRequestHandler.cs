using System;
using FluentValidation;
using MediatR;

namespace Application.UseCases.MediatR
{
   public abstract class BaseRequestHandler<TRequest, TResponse>
      : IRequestHandler<TRequest, ResponseWrapper<TResponse>>
      where TRequest : IRequest<ResponseWrapper<TResponse>>
   {
      public abstract Task<ResponseWrapper<TResponse>> Handler(TRequest request, CancellationToken cancellationToken);

      public async Task<ResponseWrapper<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
      {
         try
         {
            return await Handler(request, cancellationToken);
         }
         catch (Exception ex)
         {
            return Failed(default, ex: ex);
         }
      }

      protected ResponseWrapper<TResponse> Failed(TResponse? obj, Exception? ex = null)
      {
         return
            new ResponseWrapper<TResponse>()
            {
               Succeeded = false,
               Result = obj,
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
               ValidationMessages = messages ?? new List<string>()
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