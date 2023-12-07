using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Filters;
using PocketStorage.Core.Application.Models;
using PocketStorage.Core.Application.Queries;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class RoleController(ISender sender, ILogger<RoleController> logger) : ApiControllerBase<RoleController>(logger)
{
    [HttpGet("~/api/roles")]
    [Permit(Permission.ViewRoles)]
    [ProducesResponseType(typeof(GetRolesQueryResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetRoles([FromQuery] RoleQueryString queryString, CancellationToken cancellationToken) => ConvertToActionResult(await sender.Send(new GetRolesQuery(queryString), cancellationToken));
}
