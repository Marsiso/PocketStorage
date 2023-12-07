using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Filters;
using PocketStorage.Core.Application.Models;
using PocketStorage.Core.Application.Queries;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class UserController(ISender sender, ILogger<UserController> logger) : ApiControllerBase<UserController>(logger)
{
    [HttpGet("~/api/users")]
    [Permit(Permission.ViewUsers)]
    [ProducesResponseType(typeof(GetUsersQueryResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryString queryString, CancellationToken cancellationToken) => ConvertToActionResult(await sender.Send(new GetUsersQuery(queryString), cancellationToken));
}
