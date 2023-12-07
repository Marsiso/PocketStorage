using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.Domain.Contracts;
using static PocketStorage.Domain.Enums.RequestStatus;

namespace PocketStorage.ResourceServer.Controllers.Base;

[ApiController]
public class ApiControllerBase<TController>(ILogger<TController> logger) : ControllerBase where TController : class
{
    protected readonly ILogger<TController> Logger = logger;

    protected IActionResult ConvertToActionResult(IRequestResult response, [CallerMemberName] string? action = null) =>
        response.Status switch
        {
            Success => Ok(response),
            EntityCreated => StatusCode((int)response.Status, response),
            EntityNotFound => NotFound(response),
            Fail => BadRequest(response),
            Cancelled => StatusCode((int)response.Status, response),
            Error => RecordException(response, action)
        };

    protected IActionResult RecordException(IRequestResult response, string? action)
    {
        Logger.LogError($"Controller: `{nameof(TController)}` Action: `{action}` Message: `{response.Error?.UserFriendlyMessage}` Exception: `{response.Error?.Exception}`.");
        return StatusCode((int)response.Status, response);
    }
}
