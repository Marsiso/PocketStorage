using Microsoft.AspNetCore.Mvc;

namespace PocketStorage.ResourceServer.Controllers.Base;

[ApiController]
public abstract class ApiControllerBase<TController>(ILogger<TController> logger) : ControllerBase where TController : class
{
    protected readonly ILogger<TController> Logger = logger;
}
