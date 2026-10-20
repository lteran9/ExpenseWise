using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAPI;
using UI.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class RepaymentController : BaseController
    {
        private readonly IExpenseWiseClient _apiClient;
        private readonly ILogger<RepaymentController> _logger;

        public RepaymentController(IExpenseWiseClient apiClient, ILogger<RepaymentController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        [ImportModelState]
        public async Task<IActionResult> Ledger(Guid group, Guid? idempotencyKey, CancellationToken cancellationToken)
        {
            var model = new RepaymentLedgerViewModel
            {
                GroupKey = group,
                IdempotencyKey = idempotencyKey is { } key && key != Guid.Empty ? key : Guid.NewGuid()
            };

            try
            {
                var ledger = await _apiClient.RepaymentLedgerAsync(group, cancellationToken);
                var groupDetails = await _apiClient.GroupRetrieveAsync(group, cancellationToken);

                model.HasLoadedLedger = true;
                model.IsOrganizer = groupDetails.OwnerId == CurrentUserId;
                model.TotalSpent = ledger.TotalSpent;
                model.Outstanding = ledger.Outstanding;
                model.Members = groupDetails.Members.Select(member => new ExpenseMemberOptionViewModel
                {
                    Key = member.UniqueKey,
                    Name = member.Name
                }).ToList();
                model.Balances = ledger.Balances.Select(balance => new MemberBalanceViewModel
                {
                    Name = balance.Name,
                    NetBalance = balance.NetBalance
                }).ToList();
                model.Suggestions = ledger.Suggestions.Select(suggestion => new RepaymentSuggestionViewModel
                {
                    FromName = suggestion.FromName,
                    ToName = suggestion.ToName,
                    Amount = suggestion.Amount
                }).ToList();
                model.History = ledger.Repayments.Select(repayment => new RepaymentHistoryViewModel
                {
                    SenderName = repayment.SenderName,
                    RecipientName = repayment.RecipientName,
                    Amount = repayment.Amount,
                    Currency = repayment.Currency,
                    OccurredAt = repayment.OccurredAt
                }).ToList();
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to load the group ledger from the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while loading the group ledger.");
                ModelState.AddModelError(string.Empty, "Unable to load balances right now. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load the group ledger.");
                ModelState.AddModelError(string.Empty, "Unable to load balances right now. Please try again.");
            }

            return View(model);
        }

        [HttpPost]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(
            Guid groupKey,
            Guid senderKey,
            Guid recipientKey,
            decimal amount,
            Guid idempotencyKey,
            CancellationToken cancellationToken)
        {
            var outcomeUnknown = false;

            try
            {
                await _apiClient.RepaymentRecordAsync(new RecordRepaymentInput
                {
                    GroupKey = groupKey,
                    SenderKey = senderKey,
                    RecipientKey = recipientKey,
                    Currency = "USD",
                    Amount = amount,
                    IdempotencyKey = idempotencyKey
                }, cancellationToken);

                return RedirectToAction(nameof(Ledger), new { group = groupKey });
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to record repayment through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                outcomeUnknown = true;
                _logger.LogError(ex, "The API could not be reached while recording a repayment.");
                ModelState.AddModelError(string.Empty, "Unable to record this repayment right now. Please try again.");
            }
            catch (Exception ex)
            {
                outcomeUnknown = true;
                _logger.LogError(ex, "Unable to record the repayment.");
                ModelState.AddModelError(string.Empty, "Unable to record this repayment right now. Please try again.");
            }

            // Reuse the key only when the outcome is unknown, so a retry cannot record a duplicate.
            // A definitive rejection gets a fresh key, so a corrected amount is not treated as a reused key.
            return RedirectToAction(nameof(Ledger), new { group = groupKey, idempotencyKey = outcomeUnknown ? idempotencyKey : (Guid?)null });
        }
    }
}
