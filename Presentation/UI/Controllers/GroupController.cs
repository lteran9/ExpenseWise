using System;
using System.Threading.Tasks;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
   public class GroupController : BaseController
   {
      private readonly IMediator _mediator;
      private readonly ILogger<GroupController> _logger;

      public GroupController(IMediator mediator, ILogger<GroupController> logger)
      {
         _mediator = mediator;
         _logger = logger;
      }

      [HttpGet]
      public IActionResult Index()
      {
         var groups = new List<GroupViewModel>();

         return View(groups);
      }

      [HttpGet]
      public IActionResult Create()
      {
         var viewModel =
            new GroupViewModel()
            {
               OwnerId = Guid.Parse(HttpContext.Session.GetString("User")!)
            };
         return View(viewModel);
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> Create(GroupViewModel model)
      {
         try
         {
            var createGroupRequest =
               new CreateGroupRequest()
               {
                  Name = model.Name,
                  StartDate = model.StartDate,
                  EndDate = model.EndDate
               };

            var response = await _mediator.Send(createGroupRequest);
            if (response?.Succeeded == true)
            {
               return RedirectToAction("Index");
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex.Message);
         }

         ModelState.AddModelError(string.Empty, "Unable to create group at this time, please try again later.");

         return View();
      }
   }
}