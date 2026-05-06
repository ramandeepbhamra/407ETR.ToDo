namespace ETR.ToDo.Services.Core.Logging.Dto;

public class CreateApiLogDto
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? MethodName { get; set; }
    public string? Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int StatusCode { get; set; }
    public string? ExceptionMessage { get; set; }
}
