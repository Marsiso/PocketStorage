using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.Core.Users.Queries;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Models;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class UserInfoController(IMediator mediator, ILogger<UserInfoController> logger) : ApiControllerBase<UserInfoController>(logger)
{
    [HttpGet("~/api/userinfo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiCallResponse<UserInfo>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetUserInfoQuery(), cancellationToken));
}
