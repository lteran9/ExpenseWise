using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UI.Models;
using UI.Configuration;
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
        public IActionResult Create(Guid group)
        {
            return View(new ExpenseViewModel() { GroupKey = group });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                var userKey = CurrentUserId;

                if (userKey != Guid.Empty)
                {
                    await _apiClient.ExpenseCreateAsync(new CreateExpenseInput
                    {
                        Amount = model.Amount,
                        Currency = model.Currency,
                        Description = model.Description,
                        GroupKey = model.GroupKey
                    }, cancellationToken);

                    return RedirectToAction("Index", "Group");
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

            return View(model);
        }
    }
}
