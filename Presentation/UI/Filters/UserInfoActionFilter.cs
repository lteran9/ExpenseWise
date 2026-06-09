using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UI.Models;

namespace UI.Filters
{
    /// <summary>
    /// Action filter that automatically injects user information into ViewData for all views.
    /// This allows all views to access CurrentUser through ViewData["CurrentUser"].
    /// </summary>
    public class UserInfoActionFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoActionFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            var userIdString = httpContext.Session.GetString("User");
            var currentUser = new CurrentUserViewModel();

            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                currentUser.Id = userId;
                currentUser.IsAuthenticated = true;
            }
            else
            {
                currentUser.IsAuthenticated = false;
            }

            // Make user info available to all views
            if (context.Controller is Controller controller)
            {
                controller.ViewData["CurrentUser"] = currentUser;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after the action executes
        }
    }
}
