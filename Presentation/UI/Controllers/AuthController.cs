using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
   public class AuthController : Controller
   {
      private readonly ILogger<AuthController> _logger;

      public AuthController(ILogger<AuthController> logger)
      {
         _logger = logger;
      }

      [HttpGet]
      public IActionResult Register()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public IActionResult Register(AuthViewModel model)
      {
         ModelState.AddModelError(string.Empty, "Unable to create user. Please try again.");

         return View(model);

         //return RedirectToAction("Index", "Home");
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