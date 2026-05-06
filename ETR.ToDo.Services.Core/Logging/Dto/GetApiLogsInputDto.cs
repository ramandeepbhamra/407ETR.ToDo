using ETR.ToDo.Services.Core.Common.Dto;

namespace ETR.ToDo.Services.Core.Logging.Dto;

public class GetApiLogsInputDto : PagedAndSortedRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserName { get; set; }
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public int? StatusCode { get; set; }
    public bool? HasException { get; set; }
    public int? MinExecutionDuration { get; set; }
    public int? MaxExecutionDuration { get; set; }
}
