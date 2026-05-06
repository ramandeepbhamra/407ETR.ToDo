using ETR.ToDo.Core.Shared.Configuration;
using ETR.ToDo.Web.Core.Controllers;
using ETR.ToDo.Web.Host.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Returns public app configuration to the Angular client.
/// This endpoint is accessible without authentication.
/// </summary>
[AllowAnonymous]
public class ConfigController : ApiControllerBase
{
    private readonly ApplicationSettings _appSettings;
    private readonly SessionSettings _sessionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigController"/> class.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="sessionSettings">The session settings.</param>
    public ConfigController(
        IOptions<ApplicationSettings> appSettings,
        IOptions<SessionSettings> sessionSettings)
    {
        _appSettings = appSettings.Value;
        _sessionSettings = sessionSettings.Value;
    }

    /// <summary>
    /// Gets the public application configuration including session timeout settings, limits, and app info.
    /// </summary>
    /// <returns>An <see cref="AppConfigResponse"/> containing all public configuration.</returns>
    /// <response code="200">Returns the configuration successfully.</response>
    [HttpGet]
    public IActionResult GetConfig()
    {
        var config = new AppConfigResponse
        {
            ApplicationName = _appSettings.Name,
            ApplicationVersion = _appSettings.Version,
            SupportEmail = _appSettings.SupportEmail,
            BasicUserTaskLimit = _appSettings.Limits.BasicUserTaskLimit,
            BasicUserTaskListLimit = _appSettings.Limits.BasicUserTaskListLimit,
            IdleTimeoutMinutes = _sessionSettings.IdleTimeoutMinutes,
            WarningCountdownSeconds = _sessionSettings.WarningCountdownSeconds,
        };
        return Success(config);
    }
}
