using System;
using Api.Contracts;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepaymentController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RepaymentController> _logger;

        public RepaymentController(IMediator mediator, ILogger<RepaymentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet(nameof(Ledger))]
        [ProducesResponseType(typeof(RetrieveGroupLedgerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Ledger([FromQuery] Guid groupKey, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _mediator.Send(new RetrieveGroupLedgerRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    GroupKey = groupKey
                }, cancellationToken);

                return WrapResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve the group ledger.");
                return Problem();
            }
        }

        [HttpPost(nameof(Record))]
        [ProducesResponseType(typeof(RecordRepaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Record([FromBody] RecordRepaymentInput input, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _mediator.Send(new RecordRepaymentRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    GroupKey = input.GroupKey,
                    SenderKey = input.SenderKey,
                    RecipientKey = input.RecipientKey,
                    Currency = input.Currency,
                    Amount = input.Amount,
                    IdempotencyKey = input.IdempotencyKey
                }, cancellationToken);

                return WrapResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to record a repayment.");
                return Problem();
            }
        }
    }
}
