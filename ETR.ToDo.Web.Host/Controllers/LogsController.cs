using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Logging;
using ETR.ToDo.Services.Core.Logging.Dto;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// API log viewer — restricted to Admin and Dev roles.
/// Exposes paginated, filterable access to the <c>ApiLogs</c> database table written by Serilog.
/// </summary>
[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.Dev))]
public class LogsController : ApiControllerBase
{
    private readonly IApiLogService _apiLogService;

    public LogsController(IApiLogService apiLogService)
    {
        _apiLogService = apiLogService;
    }

    /// <summary>Returns a paginated list of API log entries with optional filters.</summary>
    /// <param name="input">Pagination, date range, log level, and keyword filters.</param>
    /// <returns>A paged result of <see cref="ApiLogDto"/>.</returns>
    /// <response code="200">Logs returned successfully.</response>
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] GetApiLogsInputDto input)
    {
        var result = await _apiLogService.GetLogsAsync(input);
        return Success(result);
    }
}
