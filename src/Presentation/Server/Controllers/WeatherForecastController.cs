using Domain.Constants;
using Domain.Data.Dtos.Get;

namespace Server.Controllers;

[ValidateAntiForgeryToken]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = $"{ApplicationConstants.Roles.SystemAdministrator}, {ApplicationConstants.Roles.TenantAdministrator}")]
[ApiController]
public sealed class WeatherForecastController : ControllerBase
{
    #region Private Fields

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    #endregion Private Fields

    #region Public Constructors

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    [HttpGet("~/api/weatherForecast")]
    public IEnumerable<WeatherForecastResource> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecastResource
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    #endregion Public Methods
}