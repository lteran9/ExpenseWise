using System;
using Application.UseCases.MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UI.Filters;
using UI.Models;

namespace UI.Controllers
{
    /// <summary>
    /// Base controller that all controllers inherit from.
    /// Automatically injects user information to all views via the UserInfoActionFilter.
    /// </summary>
    [ServiceFilter(typeof(UserInfoActionFilter))]
    public class BaseController : Controller
    {
        /// <summary>
        /// Gets the current authenticated user from ViewData.
        /// </summary>
        /// <returns>CurrentUserViewModel containing the authenticated user's information.</returns>
        protected CurrentUserViewModel GetCurrentUser()
        {
            var currentUser = ViewData["CurrentUser"] as CurrentUserViewModel;
            return currentUser ?? new CurrentUserViewModel { IsAuthenticated = false };
        }

        /// <summary>
        /// Determines if the current request is from an authenticated user.
        /// </summary>
        protected bool IsUserAuthenticated => GetCurrentUser().IsAuthenticated;

        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        protected Guid CurrentUserId => GetCurrentUser().Id;

        /// <summary>
        /// Common method to add validation errors to model state.
        /// </summary>
        protected void AddValidationErrorsToModelState<T>(ResponseWrapper<T> response) where T : class
        {
            if (response.ValidationMessages?.Any() == true)
            {
                foreach (var message in response.ValidationMessages)
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }
        }
    }
}
