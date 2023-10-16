using Microsoft.AspNetCore.Mvc;

namespace PocketStorage.IdentityServer.Controllers.Common;

public class WebControllerBase<TController> : Controller where TController : class
{
    private ILogger<TController>? _logger;

    protected ILogger<TController>? Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<TController>?>();
}
