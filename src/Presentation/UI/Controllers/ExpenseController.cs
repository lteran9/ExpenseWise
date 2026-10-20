using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UI.Models;
using UI.Configuration;
using UI.Filters;
using OpenAPI;

namespace UI.Controllers
{
    [Authorize]
    public class ExpenseController : BaseController
    {
        private readonly IExpenseWiseClient _apiClient;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseWiseClient apiClient, ILogger<ExpenseController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid group, CancellationToken cancellationToken)
        {
            var model = new ExpenseViewModel
            {
                GroupKey = group,
                PayerKey = CurrentUserId,
                IdempotencyKey = Guid.NewGuid()
            };
            await PopulateMembersAsync(model, cancellationToken);
            model.ParticipantKeys = model.Members.Select(member => member.Key).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMembersAsync(model, cancellationToken);
                return View(model);
            }

            try
            {
                if (CurrentUserId != Guid.Empty)
                {
                    if (model.IdempotencyKey == Guid.Empty)
                    {
                        ModelState.AddModelError(string.Empty, "The expense form expired. Reload it and try again.");
                        await PopulateMembersAsync(model, cancellationToken);
                        return View(model);
                    }

                    await _apiClient.ExpenseCreateAsync(new CreateExpenseInput
                    {
                        Amount = model.Amount,
                        Currency = "USD",
                        Description = model.Description,
                        GroupKey = model.GroupKey,
                        PayerKey = model.PayerKey,
                        ParticipantKeys = model.ParticipantKeys,
                        IdempotencyKey = model.IdempotencyKey
                    }, cancellationToken);

                    return RedirectToAction(nameof(List), new { group = model.GroupKey });
                }

                return Challenge();
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to create expense through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while creating an expense.");
                ModelState.AddModelError(string.Empty, "Expense creation is temporarily unavailable. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create the expense.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to create the expense, please try again.");
            }

            await PopulateMembersAsync(model, cancellationToken);
            return View(model);
        }

        [HttpGet]
        [ImportModelState]
        public async Task<IActionResult> List(Guid group, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.ExpenseListAsync(group, cancellationToken);
                var groupDetails = await _apiClient.GroupRetrieveAsync(group, cancellationToken);
                return View(new ExpenseLedgerViewModel
                {
                    GroupKey = response.GroupKey,
                    HasLoadedExpenses = true,
                    IsOrganizer = groupDetails.OwnerId == CurrentUserId,
                    Expenses = response.Expenses.Select(expense => new ExpenseLedgerItemViewModel
                    {
                        Key = expense.UniqueKey,
                        Description = expense.Description,
                        Amount = expense.Amount,
                        PayerName = expense.PayerName,
                        RecordedByName = expense.RecordedByName,
                        Voided = expense.Voided,
                        Shares = expense.Shares.Select(share => new ExpenseShareViewModel
                        {
                            ParticipantName = share.ParticipantName,
                            Amount = share.Amount
                        }).ToList()
                    }).ToList()
                });
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to load group expenses from the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while loading group expenses.");
                ModelState.AddModelError(string.Empty, "Unable to load expenses right now. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load group expenses.");
                ModelState.AddModelError(string.Empty, "Unable to load expenses right now. Please try again.");
            }

            return View(new ExpenseLedgerViewModel { GroupKey = group });
        }

        [HttpPost]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Void(Guid groupKey, Guid expenseKey, CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.ExpenseVoidAsync(new VoidExpenseInput
                {
                    GroupKey = groupKey,
                    ExpenseKey = expenseKey
                }, cancellationToken);
                return RedirectToAction(nameof(List), new { group = groupKey });
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to void expense through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while voiding an expense.");
                ModelState.AddModelError(string.Empty, "Unable to void this expense right now. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to void the expense.");
                ModelState.AddModelError(string.Empty, "Unable to void this expense right now. Please try again.");
            }

            return RedirectToAction(nameof(List), new { group = groupKey });
        }

        private async Task PopulateMembersAsync(ExpenseViewModel model, CancellationToken cancellationToken)
        {
            if (model.GroupKey == Guid.Empty)
            {
                return;
            }

            try
            {
                var group = await _apiClient.GroupRetrieveAsync(model.GroupKey, cancellationToken);
                model.Members = group.Members.Select(member => new ExpenseMemberOptionViewModel
                {
                    Key = member.UniqueKey,
                    Name = member.Name
                }).ToList();
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to load group members from the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while loading group members.");
                ModelState.AddModelError(string.Empty, "Unable to load group members right now. Please try again.");
            }
        }
    }
}
