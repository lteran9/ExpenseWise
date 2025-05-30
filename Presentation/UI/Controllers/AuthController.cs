using System;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UI.Configuration;
using UI.Models;

namespace UI.Controllers
{
   public class AuthController : BaseController
   {
      private readonly IMediator _mediator;
      private readonly ILogger<AuthController> _logger;

      public AuthController(IMediator mediator, ILogger<AuthController> logger)
      {
         _mediator = mediator;
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
         try
         {
            var createUserRequest =
               new CreateUserRequest()
               {
                  Name = $"{model.FirstName} {model.LastName}",
                  Email = model.Email,
                  Phone = model.Phone,
                  CountryCode = model.CountryCode,
                  Password = model.Password,
                  ConfirmPassword = model.ConfirmPassword
               };

            var response = await _mediator.Send(createUserRequest);
            if (response.Succeeded)
            {
               HttpContext.Session.SetString("User", response.Result!.UniqueKey.ToString());

               return RedirectToAction("Index", "Group");
            }
            else
            {
               if (response?.ValidationMessages?.Any() == true)
               {
                  AddValidationErrorsToModelState(response);

                  return View(model);
               }
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex);

            if (ex.InnerException?.Message?.Contains("users.IX_users_email") == true)
            {
               ModelState.AddModelError(string.Empty, "The email address provided is already in use.");
            }
            else if (ex.InnerException?.Message?.Contains("users.IX_users_phone") == true)
            {
               ModelState.AddModelError(string.Empty, "The phone number provided is already in use.");
            }
         }

         if (ModelState.ErrorCount == 0)
         {
            ModelState.AddModelError(string.Empty, "Unable to create user. Please try again.");
         }

         return View(model);
      }

      [HttpGet]
      public IActionResult SignIn()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> SignIn(AuthViewModel model)
      {
         try
         {
            var authenticateUserRequest =
               new AuthenticateUserRequest()
               {
                  Email = model.Email!,
                  Password = model.Password!
               };

            var response = await _mediator.Send(authenticateUserRequest);
            if (response.Succeeded)
            {
               HttpContext.Session.SetString("User", response.Result!.Id.ToString());

               return RedirectToAction("Index", "Group");
            }
            else
            {
               if (response?.ValidationMessages?.Any() == true)
               {
                  AddValidationErrorsToModelState(response);

                  return View(model);
               }
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex);
         }

         ModelState.AddModelError(string.Empty, "Unable to authenticate user. Please check your username and password.");

         return View(model);
      }

      [HttpGet]
      public IActionResult ForgotPassword()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public IActionResult ForgotPassword(AuthViewModel model)
      {
         ModelState.AddModelError(string.Empty, "Please add a valid email address.");

         return View();
      }

      [HttpGet]
      public IActionResult LogOut()
      {
         HttpContext.Session.SetString("User", string.Empty);

         return View();
      }
   }
}