using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.UseCases;

namespace ExpenseWise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(nameof(Retrieve))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Retrieve([FromRoute] FindUserRequest request)
        {
            try
            {
                return new JsonResult(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        [HttpPost(nameof(Create))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                return new JsonResult(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        [HttpPut(nameof(Update))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
        {
            try
            {
                return new JsonResult(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        [HttpDelete(nameof(Delete))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromBody] DeleteUserRequest request)
        {
            try
            {
                return new JsonResult(await _mediator.Send(request));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
    }
}
