using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UI.Models;

namespace UI.Filters
{
    /// <summary>
    /// Action filter that automatically injects user information into ViewData for all views.
    /// This allows all views to access CurrentUser through ViewData["CurrentUser"].
    /// </summary>
    public class UserInfoActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUser = new CurrentUserViewModel();
            var userIdString = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (context.HttpContext.User.Identity?.IsAuthenticated == true &&
                Guid.TryParse(userIdString, out var userId))
            {
                currentUser.Id = userId;
                currentUser.IsAuthenticated = true;
            }

            if (context.Controller is Microsoft.AspNetCore.Mvc.Controller controller)
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
