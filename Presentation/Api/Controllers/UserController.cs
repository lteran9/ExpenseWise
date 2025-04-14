using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.UseCases;

namespace ExpenseWise.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class UserController : ControllerBase
   {
      private readonly IMediator _mediator;

      public UserController(IMediator mediator)
      {
         _mediator = mediator;
      }

      [HttpGet("{id}")]
      public async Task<IActionResult> Get([FromRoute] FindUserRequest request)
      {
         try
         {
            return new JsonResult(await _mediator.Send(request));
         }
         catch (Exception ex)
         {
            return new JsonResult(ex.Message);
         }
      }

      [HttpPost]
      public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
      {
         try
         {
            return new JsonResult(await _mediator.Send(request));
         }
         catch (Exception ex)
         {
            return new JsonResult(ex.Message);
         }
      }

      [HttpDelete]
      public async Task<IActionResult> Delete([FromBody] DeleteUserRequest request)
      {
         try
         {
            return new JsonResult(await _mediator.Send(request));
         }
         catch (Exception ex)
         {
            return new JsonResult(ex.Message);
         }
      }
   }
}
