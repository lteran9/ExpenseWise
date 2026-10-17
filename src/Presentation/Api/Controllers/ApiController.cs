using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Api.Contracts;
using Application.UseCases.MediatR;

namespace Api.Controllers
{
    [Authorize]
    public abstract class ApiController : ControllerBase
    {
        protected Guid AuthenticatedUserKey
        {
            get
            {
                var userKey = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                return Guid.TryParse(userKey, out var parsedUserKey)
                    ? parsedUserKey
                    : throw new InvalidOperationException("The authenticated user identity is invalid.");
            }
        }

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
