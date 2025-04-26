using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
   public class GroupController : BaseController
   {
      public GroupController()
      {
         // Dependency Injection
      }

      [HttpGet]
      public IActionResult Index()
      {
         var groups = new List<GroupViewModel>();

         return View(groups);
      }

      [HttpGet]
      public IActionResult Create()
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