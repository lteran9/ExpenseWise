using System;
using Application.UseCases.MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
   public class BaseController : Controller
   {
      protected void AddValidationErrorsToModelState<T>(ResponseWrapper<T> response) where T : class
      {
         foreach (var message in response.ValidationMessages!)
         {
            ModelState.AddModelError(string.Empty, message);
         }
      }
   }
}