using System;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using Application.UseCases;
using MediatR;
using UI.Configuration;

namespace UI.Controllers
{
    public class ExpenseController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IMediator mediator, ILogger<ExpenseController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create(Guid group)
        {
            return View(new ExpenseViewModel() { GroupKey = group });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel model)
        {
            try
            {
                Guid.TryParse(HttpContext.Session.GetString("User"), out var userKey);

                if (userKey != Guid.Empty)
                {
                    var request =
                        new CreateExpenseRequest()
                        {
                            Amount = model.Amount,
                            Currency = model.Currency,
                            Description = model.Description,
                            GroupKey = model.GroupKey,
                            UserKey = userKey
                        };

                    var response = await _mediator.Send(request);
                    if (response.Succeeded)
                    {
                        return RedirectToAction("Index", "Group");
                    }
                    else
                    {
                        if (response.ValidationMessages?.Any() == true)
                        {
                            ModelState.AddModelError(string.Empty, response.ValidationMessages!.First());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to create the expense, please try again.");
            }

            return View(model);
        }
    }
}
