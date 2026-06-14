using System;
using Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class SeedController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeedController> _logger;

        public SeedController(IMediator mediator, ILogger<SeedController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Seed - Index action called.");

            try
            {
                // Create Users
                var requestUser1 =
                    new CreateUserRequest()
                    {
                        Name = "Luis Teran",
                        Email = "lteran@email.com",
                        Phone = "6023334578",
                        CountryCode = "+1",
                        Password = "Teran123#",
                        ConfirmPassword = "Teran123#"
                    };
                var responseUser1 = await _mediator.Send(requestUser1);

                var requestUser2 =
                    new CreateUserRequest()
                    {
                        Name = "Zinedine Zidane",
                        Email = "zizu@email.com",
                        Phone = "6023338754",
                        CountryCode = "+1",
                        Password = "Teran123#",
                        ConfirmPassword = "Teran123#"
                    };
                await _mediator.Send(requestUser2);

                var requestUser3 =
                    new CreateUserRequest()
                    {
                        Name = "Ronaldo Nazario",
                        Email = "ronaldo@email.com",
                        Phone = "6023332357",
                        CountryCode = "+1",
                        Password = "Teran123#",
                        ConfirmPassword = "Teran123#"
                    };
                await _mediator.Send(requestUser3);

                var requestGroup1 =
                    new CreateGroupRequest()
                    {
                        Name = "Sample Group",
                        OwnerId = responseUser1.Result!.Id,
                        StartDate = DateTime.Now.AddDays(3),
                        EndDate = DateTime.Now.AddDays(7)
                    };
                var responseGroup1 = await _mediator.Send(requestGroup1);

                var requestAddMember1 =
                    new AddMemberToGroupRequest()
                    {
                        GroupUniqueKey = responseGroup1.Result!.UniqueKey,
                        Phone = requestUser2.Phone,
                        CountryCode = requestUser2.CountryCode
                    };
                await _mediator.Send(requestAddMember1);

                var requestAddMember2 =
                    new AddMemberToGroupRequest()
                    {
                        GroupUniqueKey = responseGroup1.Result!.UniqueKey,
                        Phone = requestUser3.Phone,
                        CountryCode = requestUser3.CountryCode
                    };
                await _mediator.Send(requestAddMember2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("SignIn", "Auth");
        }
    }
}