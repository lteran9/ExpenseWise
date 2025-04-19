using System;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
   public class GroupController : Controller
   {
      public GroupController()
      {
         // Dependency Injection
      }

      [HttpGet]
      public IActionResult Index()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public IActionResult Create(GroupViewModel model)
      {
         return View();
      }
   }
}