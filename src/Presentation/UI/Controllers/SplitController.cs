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
    public class SplitController : BaseController
    {
        private readonly IExpenseWiseClient _apiClient;
        private readonly ILogger<SplitController> _logger;

        public SplitController(IExpenseWiseClient apiClient, ILogger<SplitController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        [ImportModelState]
        public async Task<IActionResult> Index(Guid group, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.ExpenseListAsync(group, cancellationToken);
                return View(new SplitViewModel
                {
                    GroupKey = response.GroupKey,
                    GroupMemberCount = response.GroupMembers,
                    ExpenseList = response.Expenses.Select(expense => new ExpenseViewModel
                    {
                        Settled = false,
                        Description = expense.Description,
                        Currency = expense.Currency,
                        Amount = (decimal)expense.Amount,
                        GroupKey = response.GroupKey
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

            return View(new SplitViewModel { GroupKey = group, ExpenseList = [] });
        }

        [HttpPost]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SplitViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.ExpenseSplitAsync(new SplitExpenseInput
                {
                    GroupKey = model.GroupKey
                }, cancellationToken);
                if (response.Success)
                {
                    return RedirectToAction("Index", "Group");
                }
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to split expenses through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached while splitting expenses.");
                ModelState.AddModelError(string.Empty, "Unable to take payment at this time.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to split expenses.");
                ModelState.AddModelError(string.Empty, "Unable to take payment at this time.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to take payment at this time.");
            }

            return RedirectToAction("Index", "Split", new { group = model.GroupKey });
        }
    }
}