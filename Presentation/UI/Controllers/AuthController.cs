using System.Diagnostics;
using System.Threading.Tasks;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
   public class AuthController : Controller
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
                  Email = model.Email!,
                  Phone = model.Phone!,
                  Password = model.Password!,
                  ConfirmPassword = model.ConfirmPassword!
               };

            var response = await _mediator.Send(createUserRequest);
            if (response.Succeeded)
            {
               return RedirectToAction("Index", "Group");
            }
            else
            {
               if (response?.ValidationMessages?.Any() == true)
               {
                  foreach (var message in response.ValidationMessages)
                  {
                     ModelState.AddModelError(string.Empty, message);
                  }

                  return View(model);
               }
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex.Message);
         }

         ModelState.AddModelError(string.Empty, "Unable to create user. Please try again.");

         return View(model);
      }

      [HttpGet]
      public IActionResult SignIn()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public IActionResult SignIn(AuthViewModel model)
      {

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
   }
}