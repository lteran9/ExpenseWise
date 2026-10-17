using System;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Contracts;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IMediator mediator, ILogger<ExpenseController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet(nameof(List))]
        [ProducesResponseType(typeof(ExpenseListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] Guid groupKey, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _mediator.Send(new ListExpensesRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    GroupKey = groupKey
                }, cancellationToken);
                if (!response.Succeeded || response.Result == null)
                {
                    return BadRequest(response);
                }

                return Ok(new ExpenseListResponse(
                    response.Result.GroupKey,
                    response.Result.GroupMembers,
                    response.Result.Expenses.Select(expense => new ExpenseListItemResponse(
                        expense.Id,
                        expense.Description,
                        expense.Currency,
                        expense.Amount)).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to list group expenses.");
                return Problem();
            }
        }

        [HttpPost(nameof(Create))]
        [ProducesResponseType(typeof(CreateExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateExpenseInput input, CancellationToken cancellationToken)
        {
            try
            {
                var request = new CreateExpenseRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    Description = input.Description,
                    Currency = input.Currency,
                    Amount = input.Amount,
                    GroupKey = input.GroupKey
                };
                var response = await _mediator.Send(request, cancellationToken);
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
                _logger.LogError(ex, "Unable to create an expense.");
                return Problem();
            }
        }

        [HttpPost(nameof(Split))]
        [ProducesResponseType(typeof(SplitExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Split([FromBody] SplitExpenseInput input, CancellationToken cancellationToken)
        {
            try
            {
                var request = new SplitExpenseRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    GroupKey = input.GroupKey
                };
                var response = await _mediator.Send(request, cancellationToken);
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
                _logger.LogError(ex, "Unable to split expenses.");
                return Problem();
            }
        }

        [HttpPost(nameof(Pay))]
        [ProducesResponseType(typeof(PayExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Pay()
        {
            try
            {
                var response = await _mediator.Send(new PayExpenseRequest());
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
                _logger.LogError(ex, "Unable to record an expense payment.");
                return Problem();
            }
        }

        [HttpPut(nameof(Update))]
        [ProducesResponseType(typeof(UpdateExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update()
        {
            try
            {
                var response = await _mediator.Send(new UpdateExpenseRequest());
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
                _logger.LogError(ex, "Unable to update an expense.");
                return Problem();
            }
        }
    }
}
