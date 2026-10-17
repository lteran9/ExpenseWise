using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.UseCases;
using Api.Contracts;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GroupController> _logger;

        public GroupController(IMediator mediator, ILogger<GroupController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet(nameof(Retrieve))]
        [ProducesResponseType(typeof(RetrieveGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Retrieve([FromQuery] Guid uniqueKey)
        {
            try
            {
                var request = new RetrieveGroupRequest
                {
                    UniqueKey = uniqueKey,
                    ActorKey = AuthenticatedUserKey
                };
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
                _logger.LogError(ex, "Unable to retrieve a group.");
                return Problem();
            }
        }

        [HttpGet(nameof(List))]
        [ProducesResponseType(typeof(ListGroupsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List()
        {
            try
            {
                var request = new ListGroupsRequest { ActorKey = AuthenticatedUserKey };
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
                _logger.LogError(ex, "Unable to list groups.");
                return Problem();
            }
        }

        [HttpPost(nameof(Create))]
        [ProducesResponseType(typeof(CreateGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateGroupInput input)
        {
            try
            {
                var request = new CreateGroupRequest
                {
                    ActorKey = AuthenticatedUserKey,
                    Name = input.Name,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate
                };
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
                _logger.LogError(ex, "Unable to create a group.");
                return Problem();
            }
        }

        [HttpPost(nameof(Update))]
        [ProducesResponseType(typeof(UpdateGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateGroupInput input)
        {
            try
            {
                var authorizationResult = await RequireOrganizer(input.UniqueKey);
                if (authorizationResult != null)
                {
                    return authorizationResult;
                }

                var request = new UpdateGroupRequest
                {
                    UniqueKey = input.UniqueKey,
                    ActorKey = AuthenticatedUserKey,
                    Name = input.Name,
                    Active = input.Active,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate
                };
                var response = await _mediator.Send(request);
                if (response.Succeeded)
                {
                    return Ok(response.Result);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update a group.");
                return Problem();
            }
        }

        [HttpPost(nameof(AddMember))]
        [ProducesResponseType(typeof(AddMemberToGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMember([FromBody] AddGroupMemberInput input)
        {
            try
            {
                var authorizationResult = await RequireOrganizer(input.GroupUniqueKey);
                if (authorizationResult != null)
                {
                    return authorizationResult;
                }

                var request = new AddMemberToGroupRequest
                {
                    GroupUniqueKey = input.GroupUniqueKey,
                    ActorKey = AuthenticatedUserKey,
                    Phone = input.Phone,
                    CountryCode = input.CountryCode
                };
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
                _logger.LogError(ex, "Unable to add a group member.");
                return Problem();
            }
        }

        [HttpDelete(nameof(Delete))]
        [ProducesResponseType(typeof(DeleteGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] ArchiveGroupInput input)
        {
            try
            {
                var authorizationResult = await RequireOrganizer(input.GroupKey);
                if (authorizationResult != null)
                {
                    return authorizationResult;
                }

                var request = new DeleteGroupRequest
                {
                    GroupKey = input.GroupKey,
                    ActorKey = AuthenticatedUserKey
                };
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
                _logger.LogError(ex, "Unable to archive a group.");
                return Problem();
            }
        }

        private async Task<IActionResult?> RequireOrganizer(Guid groupKey)
        {
            var response = await _mediator.Send(new RetrieveGroupRequest
            {
                UniqueKey = groupKey,
                ActorKey = AuthenticatedUserKey
            });
            if (!response.Succeeded || response.Result == null)
            {
                return NotFound();
            }

            return response.Result.OwnerId == AuthenticatedUserKey ? null : Forbid();
        }
    }
}
