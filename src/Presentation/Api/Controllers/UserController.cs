using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.UseCases;
using Api.Contracts;
using Api.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;
        private readonly AccessTokenIssuer _tokenIssuer;

        public UserController(IMediator mediator, ILogger<UserController> logger, AccessTokenIssuer tokenIssuer)
        {
            _mediator = mediator;
            _logger = logger;
            _tokenIssuer = tokenIssuer;
        }

        [HttpGet(nameof(Find))]
        [ProducesResponseType(typeof(FindUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find()
        {
            try
            {
                var request = new FindUserRequest { ActorKey = AuthenticatedUserKey };
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to find the requested user.");
            }

            return Problem();
        }

        [HttpPost(nameof(Create))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create a new user.");
            }

            return Problem();
        }

        [HttpPost(nameof(Authenticate))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded && response.Result != null)
                {
                    var (token, expiresAtUtc) = _tokenIssuer.Issue(response.Result.Id);
                    response.Result.AccessToken = token;
                    response.Result.ExpiresAtUtc = expiresAtUtc;
                }

                return WrapResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to authenticate user.");
            }

            return Problem();
        }

        // [HttpPost(nameof(ForgotPassword))]
        // [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        // {
        //    try
        //         {
        //             var response = await _mediator.Send(request);
        //             if (response.Succeeded)
        //             {
        //                 return Ok(response.Result);
        //             }
        //             else
        //             {
        //                 return BadRequest(response);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             return Problem(detail: ex.Message);
        //         }
        // }

        [HttpPut(nameof(Update))]
        [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateUserInput input)
        {
            try
            {
                var request = new UpdateUserRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    Name = input.Name,
                    Email = input.Email,
                    Phone = input.Phone,
                    CountryCode = input.CountryCode
                };
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update the user information.");
            }

            return Problem();
        }

        [HttpDelete(nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Delete()
        {
            _logger.LogInformation("Account deletion was rejected because it is not supported in Phase 1.");
            return Conflict(new { message = "Account deletion is not supported in Phase 1." });
        }
    }
}
