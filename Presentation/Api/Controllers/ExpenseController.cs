using System;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : Controller
    {
        private readonly IMediator _mediator;

        public ExpenseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost(nameof(Create))]
        [ProducesResponseType(typeof(CreateExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromQuery] CreateExpenseRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPost(nameof(Split))]
        [ProducesResponseType(typeof(SplitExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Split([FromBody] SplitExpenseRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPost(nameof(Pay))]
        [ProducesResponseType(typeof(PayExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Pay([FromBody] PayExpenseRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPut(nameof(Update))]
        [ProducesResponseType(typeof(UpdateExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateExpenseRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }
    }
}
