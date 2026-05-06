using ETR.ToDo.Services.Core.Common.Dto;
using ETR.ToDo.Services.Core.Logging.Dto;

namespace ETR.ToDo.Services.Core.Logging;

/// <summary>
/// Service contract for reading and writing API log entries stored in the <c>ApiLogs</c> database table.
/// Log entries are written by the Serilog pipeline and read back via the <c>LogsController</c>.
/// </summary>
public interface IApiLogService
{
    /// <summary>Persists a new API log entry.</summary>
    /// <param name="dto">The log entry data captured from the request pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CreateAsync(CreateApiLogDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated, filterable list of API log entries.</summary>
    /// <param name="input">Pagination, date range, log level, and keyword filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of <see cref="ApiLogDto"/>.</returns>
    Task<PagedResultDto<ApiLogDto>> GetLogsAsync(GetApiLogsInputDto input, CancellationToken cancellationToken = default);
}
