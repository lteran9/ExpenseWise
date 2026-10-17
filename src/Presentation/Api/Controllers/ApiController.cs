using System;
using Microsoft.AspNetCore.Mvc;
using Api.Contracts;
using Application.UseCases.MediatR;

namespace Api.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        protected virtual IActionResult WrapResponse<T>(ResponseWrapper<T> response)
        {
            if (response.Succeeded)
            {
                return Ok(response.Result);
            }
            else
            {
                return BadRequest(response.ValidationMessages!);
            }
        }

        protected virtual BadRequestObjectResult BadRequest(List<string> validationMessages)
        {
            return BadRequest(new ValidationErrorResponse() { ValidationMessages = validationMessages });
        }
    }
}
