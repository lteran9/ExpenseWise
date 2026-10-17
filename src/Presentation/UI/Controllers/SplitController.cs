using System;
using MediatR;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Configuration;
using UI.Filters;

namespace UI.Controllers
{
    public class SplitController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SplitController> _logger;

        public SplitController(IMediator mediator, ILogger<SplitController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ImportModelState]
        public async Task<IActionResult> Index(Guid group)
        {
            var request = new ListExpensesRequest() { GroupKey = group };
            var response = await _mediator.Send(request);
            var expenseList = response?.Result?.Expenses;
            if (response != null && expenseList?.Any() == true)
            {
                var viewModel =
                    new SplitViewModel()
                    {
                        GroupKey = response.Result!.GroupKey,
                        GroupMemberCount = response.Result!.GroupMembers,
                        ExpenseList = expenseList.Select(ModelMapper.Instance.Map<ExpenseViewModel>)
                    };

                return View(viewModel);
            }

            return View();
        }

        [HttpPost]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SplitViewModel model)
        {
            try
            {
                var request =
                    new SplitExpenseRequest()
                    {
                        GroupKey = model.GroupKey,
                        UserKey = GetCurrentUser().Id
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
                        ModelState.AddModelError(string.Empty, response.ValidationMessages.First());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException?.Message ?? ex.Message);
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to take payment at this time.");
            }

            return RedirectToAction("Index", "Split", new { group = model.GroupKey });
        }
    }
}