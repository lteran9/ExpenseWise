using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.UseCases;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(nameof(Retrieve))]
        [ProducesResponseType(typeof(RetrieveGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Retrieve([FromQuery] RetrieveGroupRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpGet(nameof(List))]
        [ProducesResponseType(typeof(ListGroupsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List([FromQuery] ListGroupsRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPost(nameof(Create))]
        [ProducesResponseType(typeof(CreateGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPost(nameof(Update))]
        [ProducesResponseType(typeof(UpdateGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateGroupRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpPost(nameof(AddMember))]
        [ProducesResponseType(typeof(AddMemberToGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMember([FromBody] AddMemberToGroupRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [HttpDelete(nameof(Delete))]
        [ProducesResponseType(typeof(DeleteGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] DeleteGroupRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }
    }
}
