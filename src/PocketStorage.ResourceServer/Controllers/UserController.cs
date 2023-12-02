using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Filters;
using PocketStorage.Core.Application.Queries;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class UserController(ISender sender, ILogger<UserController> logger) : ApiControllerBase<UserController>(logger)
{
    [HttpGet("~/api/users")]
    [Permit(Permission.ViewUsers)]
    [ProducesResponseType(typeof(GetUsersQueryResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetUsersQueryResult), StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetUsersQueryResult), 499, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetUsersQueryResult), StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetAllUser(CancellationToken cancellationToken) => ConvertToActionResult(await sender.Send(new GetUsersQuery(), cancellationToken));
}
