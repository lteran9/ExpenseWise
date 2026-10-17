using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenAPI;
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

        protected void AddApiErrorsToModelState(ApiException exception)
        {
            if (!string.IsNullOrWhiteSpace(exception.Response))
            {
                try
                {
                    using var document = JsonDocument.Parse(exception.Response);
                    var root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var message in root.EnumerateArray())
                        {
                            if (message.ValueKind == JsonValueKind.String && message.GetString() is { } text)
                            {
                                ModelState.AddModelError(string.Empty, text);
                            }
                        }
                    }

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (TryGetProperty(root, "validationMessages", out var validationMessages) &&
                            validationMessages.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var message in validationMessages.EnumerateArray())
                            {
                                if (message.ValueKind == JsonValueKind.String && message.GetString() is { } text)
                                {
                                    ModelState.AddModelError(string.Empty, text);
                                }
                            }
                        }

                        if (TryGetProperty(root, "detail", out var detail) &&
                            detail.ValueKind == JsonValueKind.String &&
                            detail.GetString() is { Length: > 0 } detailText)
                        {
                            ModelState.AddModelError(string.Empty, detailText);
                        }
                    }
                }
                catch
                {
                    // Fall back to a concise status message for non-JSON error bodies.
                }
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, $"The API request failed with status code {exception.StatusCode}.");
            }
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
