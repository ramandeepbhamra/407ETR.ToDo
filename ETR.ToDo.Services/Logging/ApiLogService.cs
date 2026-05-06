using ETR.ToDo.Core.Entities.Logging;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Services.Core.Common.Dto;
using ETR.ToDo.Services.Core.Logging;
using ETR.ToDo.Services.Core.Logging.Dto;
using ETR.ToDo.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ETR.ToDo.Services.Logging;

public class ApiLogService : IApiLogService
{
    private readonly IRepository<ApiLog, Guid> _repository;
    private readonly ApiLogMapper _mapper;

    public ApiLogService(IRepository<ApiLog, Guid> repository, ApiLogMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task CreateAsync(CreateApiLogDto dto, CancellationToken cancellationToken = default)
    {
        var log = _mapper.CreateDtoToApiLog(dto);
        log.Id = Guid.NewGuid();
        await _repository.InsertAsync(log, cancellationToken);
    }

    public async Task<PagedResultDto<ApiLogDto>> GetLogsAsync(GetApiLogsInputDto input, CancellationToken cancellationToken = default)
    {
        var query = _repository.GetAll();

        if (input.StartDate.HasValue)
            query = query.Where(e => e.ExecutionTime >= input.StartDate.Value);

        if (input.EndDate.HasValue)
            query = query.Where(e => e.ExecutionTime <= input.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(input.UserName))
            query = query.Where(e => e.UserName != null && e.UserName.Contains(input.UserName));

        if (!string.IsNullOrWhiteSpace(input.ServiceName))
            query = query.Where(e => e.ServiceName.Contains(input.ServiceName));

        if (!string.IsNullOrWhiteSpace(input.MethodName))
            query = query.Where(e => e.MethodName != null && e.MethodName.Contains(input.MethodName));

        if (input.StatusCode.HasValue)
            query = query.Where(e => e.StatusCode == input.StatusCode.Value);

        if (input.HasException.HasValue)
            query = input.HasException.Value
                ? query.Where(e => e.ExceptionMessage != null)
                : query.Where(e => e.ExceptionMessage == null);

        if (input.MinExecutionDuration.HasValue)
            query = query.Where(e => e.ExecutionDuration >= input.MinExecutionDuration.Value);

        if (input.MaxExecutionDuration.HasValue)
            query = query.Where(e => e.ExecutionDuration <= input.MaxExecutionDuration.Value);

        query = (input.SortBy?.ToLower(), input.SortDirection?.ToLower()) switch
        {
            ("executionduration", "asc")  => query.OrderBy(e => e.ExecutionDuration),
            ("executionduration", _)      => query.OrderByDescending(e => e.ExecutionDuration),
            ("statuscode", "asc")         => query.OrderBy(e => e.StatusCode),
            ("statuscode", _)             => query.OrderByDescending(e => e.StatusCode),
            ("username", "asc")           => query.OrderBy(e => e.UserName),
            ("username", _)               => query.OrderByDescending(e => e.UserName),
            ("servicename", "asc")        => query.OrderBy(e => e.ServiceName),
            ("servicename", _)            => query.OrderByDescending(e => e.ServiceName),
            (_, "asc")                    => query.OrderBy(e => e.ExecutionTime),
            _                             => query.OrderByDescending(e => e.ExecutionTime),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ApiLogDto>
        {
            Items = _mapper.ApiLogsToDtos(items),
            TotalCount = totalCount,
            Page = input.Page,
            PageSize = input.PageSize,
        };
    }
}
