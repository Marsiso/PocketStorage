using Microsoft.AspNetCore.Mvc;

namespace PocketStorage.ResourceServer.Controllers.Base;

[ApiController]
public abstract class ApiControllerBase<TController> : ControllerBase where TController : class
{
    protected readonly ILogger<TController> Logger;

    public ApiControllerBase(ILogger<TController> logger) => Logger = logger;
}
