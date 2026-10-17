using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.UseCases;
using Api.Contracts;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet(nameof(Find))]
        [ProducesResponseType(typeof(FindUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Find([FromQuery] FindUserRequest request)
        {
            try
            {
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to find the requested user.");
            }

            return Problem();
        }

        [HttpPost(nameof(Create))]
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
        [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
        {
            try
            {
                return WrapResponse(await _mediator.Send(request));
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
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
        {
            try
            {
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update the user information.");
            }

            return Problem();
        }

        [HttpDelete(nameof(Delete))]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] DeleteUserRequest request)
        {
            try
            {
                return WrapResponse(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to delete user.");
            }

            return Problem();
        }
    }
}
