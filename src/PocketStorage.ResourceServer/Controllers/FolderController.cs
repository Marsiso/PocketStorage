using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Filters;
using PocketStorage.Core.Files.Queries;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class FolderController(ISender sender, ILogger<FolderController> logger) : ApiControllerBase<FolderController>(logger)
{
    [HttpGet("~/api/user/{userId}/folder/{folderId}")]
    [Permit(Permission.ViewFiles)]
    [ProducesResponseType(typeof(FindFolderQueryResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetFolder(string? userId, int? folderId, CancellationToken cancellationToken) => ConvertToActionResult(await sender.Send(new FindFolderQuery(userId, folderId), cancellationToken));

    [HttpGet("~/api/folder/{folderId}")]
    [Permit]
    [ProducesResponseType(typeof(GetFolderQueryResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetFolder(int? folderId, CancellationToken cancellationToken) => ConvertToActionResult(await sender.Send(new GetFolderQuery(folderId), cancellationToken));
}
