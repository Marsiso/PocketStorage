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
    /// <summary>
    ///     Retrieves consented claims about the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Claims associated with the current user.</returns>
    [HttpGet("~/api/userinfo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiCallResponse<UserInfo>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiCallResponse<UserInfo>), StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiCallResponse<UserInfo>), StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiCallResponse<UserInfo>), StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken) => ConvertToActionResult(await mediator.Send(new GetUserInfoQuery(), cancellationToken));
}
