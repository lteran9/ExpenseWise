using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.UseCases;

namespace Api.Controllers
{
    public class GroupController : Controller
    {
        private readonly IMediator _mediator;

        public GroupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest group)
        {
            try
            {
                return new JsonResult(await _mediator.Send(group));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
    }
}
