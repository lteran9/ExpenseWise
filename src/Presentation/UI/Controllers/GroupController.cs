using OpenAPI;
using Microsoft.AspNetCore.Mvc;
using UI.Configuration;
using UI.Models;

namespace UI.Controllers
{
    public class GroupController : BaseController
    {
        private readonly IExpenseWiseClient _apiClient;
        private readonly ILogger<GroupController> _logger;

        public GroupController(IExpenseWiseClient apiClient, ILogger<GroupController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.GroupListAsync(CurrentUserId);
                return View(response.Groups.Select(ModelMapper.Instance.Map<GroupViewModel>));
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to load groups from the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load groups.");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new GroupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model)
        {
            try
            {
                var user = await _apiClient.UserFindAsync(CurrentUserId);
                await _apiClient.GroupCreateAsync(
                    new CreateGroupRequest
                    {
                        Name = model.Name,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        OwnerId = user.Id
                    });

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to create group through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create group.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to create group at this time, please try again later.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid key)
        {
            try
            {
                var response = await _apiClient.GroupRetrieveAsync(key);
                return View(ModelMapper.Instance.Map<GroupViewModel>(response));
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to retrieve group from the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve group.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel model)
        {
            try
            {
                await _apiClient.GroupUpdateAsync(
                    new UpdateGroupRequest
                    {
                        UniqueKey = model.UniqueKey,
                        Name = model.Name,
                        Active = model.Active,
                        StartDate = model.StartDate ?? DateTime.MinValue,
                        EndDate = model.EndDate ?? DateTime.MinValue
                    });

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to update group through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update group.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to edit group at this time, please try again later.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Invite(Guid key)
        {
            try
            {
                var response = await _apiClient.GroupRetrieveAsync(key);
                return View(new GroupInviteViewModel
                {
                    Group = ModelMapper.Instance.Map<GroupViewModel>(response)
                });
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to retrieve group for invite. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to retrieve group for invite.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(GroupInviteViewModel model)
        {
            try
            {
                var response = await _apiClient.GroupAddMemberAsync(
                    new AddMemberToGroupRequest
                    {
                        Phone = model.Phone,
                        CountryCode = model.CountryCode,
                        GroupUniqueKey = model.Group.UniqueKey
                    });

                if (response.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Unable to invite group member through the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to invite group member.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to invite member to group at this time, please try again later.");
            }

            return View(model);
        }

    }
}
