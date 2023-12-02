using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;

namespace PocketStorage.ResourceServer.Controllers.Base;

[ApiController]
public abstract class ApiControllerBase<TController>(ILogger<TController> logger) : ControllerBase where TController : class
{
    protected readonly ILogger<TController> Logger = logger;

    protected IActionResult ConvertToActionResult(IRequestResult response, [CallerMemberName] string? action = null) =>
        response.Status switch
        {
            RequestStatus.Success => Ok(response),
            RequestStatus.EntityCreated => StatusCode((int)response.Status, response),
            RequestStatus.EntityNotFound => NotFound(response),
            RequestStatus.Fail => BadRequest(response),
            RequestStatus.Cancelled => StatusCode((int)response.Status, response),
            RequestStatus.Error => RecordException(response, action)
        };

    protected IActionResult RecordException(IRequestResult response, string? action)
    {
        Logger.LogError($"Controller: `{nameof(TController)}` Action: `{action}` Message: `{response.Error?.UserFriendlyMessage}` Exception: `{response.Error?.Exception}`.");
        return StatusCode((int)response.Status, response);
    }
}
