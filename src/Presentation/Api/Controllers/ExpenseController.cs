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
                    response.Result.Expenses.Select(ToExpenseListItem).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to list group expenses.");
                return Problem();
            }
        }

        [HttpGet(nameof(Detail))]
        [ProducesResponseType(typeof(ExpenseListItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Detail([FromQuery] Guid expenseKey, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _mediator.Send(new RetrieveExpenseRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    ExpenseKey = expenseKey
                }, cancellationToken);
                if (!response.Succeeded || response.Result == null)
                {
                    return BadRequest(response);
                }

                return Ok(ToExpenseListItem(response.Result.Expense));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve an expense.");
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
                    GroupKey = input.GroupKey,
                    PayerKey = input.PayerKey,
                    ParticipantKeys = input.ParticipantKeys,
                    IdempotencyKey = input.IdempotencyKey
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

        [HttpPost(nameof(Void))]
        [ProducesResponseType(typeof(VoidExpenseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Void([FromBody] VoidExpenseInput input, CancellationToken cancellationToken)
        {
            try
            {
                var request = new VoidExpenseRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    GroupKey = input.GroupKey,
                    ExpenseKey = input.ExpenseKey
                };
                var response = await _mediator.Send(request, cancellationToken);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to void an expense.");
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

        private static ExpenseListItemResponse ToExpenseListItem(Core.Entities.Expense expense) =>
            new(
                expense.UniqueKey,
                expense.Id,
                expense.Description,
                expense.Currency,
                expense.Amount,
                expense.Payer!.UniqueKey,
                expense.Payer.Name,
                expense.CreatedBy!.UniqueKey,
                expense.CreatedBy.Name,
                expense.Voided,
                expense.Shares.Select(share => new ExpenseShareResponse(
                    share.Participant.UniqueKey,
                    share.Participant.Name,
                    share.Amount)).ToList());
    }
}
