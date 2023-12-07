using Microsoft.AspNetCore.Mvc;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class GrafanaController(ILogger<GrafanaController> logger) : ApiControllerBase<GrafanaController>(logger)
{
    [HttpGet("~/startup")]
    public IActionResult GetGrafanaRoute(IConfiguration configuration) => new ObjectResult(new { GrafanaUrl = (string)configuration["GRAFANA_URL"] });
}
