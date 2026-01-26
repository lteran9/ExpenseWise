using System;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UI.Configuration;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                var request =
                   new ListGroupsRequest()
                   {
                       UserId = new Guid(HttpContext.Session.GetString("User")!)
                   };

                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    var groups = response.Result?.Groups;
                    if (groups?.Any() == true)
                    {
                        return View(groups.Select(ModelMapper.GroupMapper.Map<GroupViewModel>));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            return View();
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
                var user =
                   new FindUserRequest()
                   {
                       UniqueKey = model.OwnerId
                   };
                var userResponse = await _mediator.Send(user);

                var createGroupRequest =
                   new CreateGroupRequest()
                   {
                       Name = model.Name,
                       StartDate = model.StartDate,
                       EndDate = model.EndDate,
                       OwnerId = userResponse.Result!.Id
                   };

                var response = await _mediator.Send(createGroupRequest);
                if (response?.Succeeded == true)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            ModelState.AddModelError(string.Empty, "Unable to create group at this time, please try again later.");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid key)
        {
            var request =
               new RetrieveGroupRequest()
               {
                   UniqueKey = key
               };

            var response = await _mediator.Send(request);
            if (response.Succeeded)
            {
                var groupEntity = response.Result;
                if (groupEntity != null)
                {
                    return View(ModelMapper.GroupMapper.Map<GroupViewModel>(groupEntity));
                }
                else
                {
                    _logger.LogInformation("Group entity in successful response was null.");
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Invite(Guid key)
        {
            var request =
               new RetrieveGroupRequest()
               {
                   UniqueKey = key
               };

            var response = await _mediator.Send(request);
            if (response.Succeeded)
            {
                var groupEntity = response.Result;
                if (groupEntity != null)
                {
                    var viewModel =
                       new GroupInviteViewModel()
                       {
                           Group = ModelMapper.GroupMapper.Map<GroupViewModel>(groupEntity)
                       };

                    return View(viewModel);
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(GroupInviteViewModel model)
        {
            try
            {
                var addMemberRequest =
                   new AddMemberToGroupRequest()
                   {
                       Phone = model.Phone,
                       CountryCode = model.CountryCode,
                       GroupUniqueKey = model.Group.UniqueKey
                   };

                var response = await _mediator.Send(addMemberRequest);
                if (response.Succeeded)
                {
                    if (response.Result?.Success == true)
                    {
                        // Member added to group
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    if (response.ValidationMessages?.Any() == true)
                    {
                        // Unable to add user to group
                        ModelState.AddModelError(string.Empty, response.ValidationMessages.First());

                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }

            ModelState.AddModelError(string.Empty, "Unable to invite member to group at this time, please try again later.");

            return View(model);
        }
    }
}
