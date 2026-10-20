using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OpenAPI;
using UI.Configuration;
using UI.Models;

namespace UI.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IExpenseWiseClient _apiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IExpenseWiseClient apiClient, ILogger<AuthController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AuthViewModel model)
        {
            var accountCreated = false;
            try
            {
                await _apiClient.UserCreateAsync(new CreateUserRequest
                {
                    Name = $"{model.FirstName} {model.LastName}",
                    Email = model.Email,
                    Phone = model.Phone,
                    CountryCode = model.CountryCode,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword
                });
                accountCreated = true;

                var authentication = await _apiClient.UserAuthenticateAsync(
                    new AuthenticateUserRequest { Email = model.Email, Password = model.Password });
                if (await EstablishSession(authentication))
                {
                    return RedirectToAction("Index", "Group");
                }

                ModelState.AddModelError(string.Empty, "Your account was created. Please sign in to continue.");
            }
            catch (ApiException ex)
            {
                if (accountCreated)
                {
                    _logger.LogError(ex, "The account was created but sign-in did not complete.");
                    ModelState.AddModelError(string.Empty, "Your account was created. Please sign in to continue.");
                }
                else
                {
                    AddApiErrorsToModelState(ex);
                    _logger.LogWarning("Registration was rejected by the API. Status code: {StatusCode}", ex.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                if (accountCreated)
                {
                    _logger.LogError(ex, "The account was created but sign-in could not reach the API.");
                    ModelState.AddModelError(string.Empty, "Your account was created. Please sign in to continue.");
                }
                else
                {
                    _logger.LogError(ex, "The API could not be reached during registration.");
                    ModelState.AddModelError(string.Empty, "Registration is temporarily unavailable. Please try again.");
                }
            }

            model.Password = string.Empty;
            model.ConfirmPassword = string.Empty;
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            return View(model);
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(AuthViewModel model, string? returnUrl = null)
        {
            try
            {
                var response = await _apiClient.UserAuthenticateAsync(
                    new AuthenticateUserRequest { Email = model.Email, Password = model.Password });
                if (await EstablishSession(response))
                {
                    return Url.IsLocalUrl(returnUrl)
                        ? LocalRedirect(returnUrl!)
                        : RedirectToAction("Index", "Group");
                }
            }
            catch (ApiException ex)
            {
                AddApiErrorsToModelState(ex);
                _logger.LogWarning("Sign-in was rejected by the API. Status code: {StatusCode}", ex.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The API could not be reached during sign-in.");
                ModelState.AddModelError(string.Empty, "Sign-in is temporarily unavailable. Please try again.");
            }

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to sign in. Please check your email and password.");
            }

            model.Password = string.Empty;
            ModelState.Remove(nameof(model.Password));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Remove(ApiTokenHandler.SessionKey);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(SignIn));
        }

        private async Task<bool> EstablishSession(AuthenticateUserResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.AccessToken) || response.Id == Guid.Empty)
            {
                _logger.LogError("The API returned an incomplete authentication response.");
                return false;
            }

            HttpContext.Session.SetString(ApiTokenHandler.SessionKey, response.AccessToken);
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, response.Id.ToString())],
                CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = new DateTimeOffset(response.ExpiresAtUtc, TimeSpan.Zero)
                });

            return true;
        }
    }
}
