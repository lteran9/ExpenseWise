using System;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.SqlDatabase;

namespace ExpenseWise.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class UserController : ControllerBase
   {
      private readonly IRepository _repository;

      public UserController(IRepository repository)
      {
         _repository = repository;
      }

      [HttpGet("{id}")]
      public async Task<IActionResult> Get(int id)
      {
         try
         {
            var dbUser = await _repository.GetAsync(new Core.Entities.User() { Id = id });

            if (dbUser != null)
            {
               return new JsonResult(dbUser);
            }
         }
         catch (Exception ex)
         {
            return new JsonResult(ex.Message);
         }

         return StatusCode(500);
      }
   }
}
