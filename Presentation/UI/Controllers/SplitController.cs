using System;
using MediatR;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Configuration;

namespace UI.Controllers
{
    public class SplitController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GroupController> _logger;

        public SplitController(IMediator mediator, ILogger<GroupController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SplitViewModel model)
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

            return RedirectToAction("Index", "Split", new { group = model.GroupKey });
        }
    }
}