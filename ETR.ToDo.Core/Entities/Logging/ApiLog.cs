using ETR.ToDo.Core.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETR.ToDo.Core.Entities.Logging;

[Table("ApiLogs")]
public class ApiLog : BaseEntity<Guid>
{
    public const int MaxHttpMethodLength = 10;
    public const int MaxServiceNameLength = 100;
    public const int MaxMethodNameLength = 100;
    public const int MaxParametersLength = 4000;
    public const int MaxClientIpAddressLength = 45;
    public const int MaxUserAgentLength = 500;
    public const int MaxExceptionMessageLength = 2000;
    public const int MaxUserNameLength = 256;

    public Guid? UserId { get; set; }

    [MaxLength(MaxUserNameLength)]
    public string? UserName { get; set; }

    [Required]
    [MaxLength(MaxHttpMethodLength)]
    public string HttpMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(MaxServiceNameLength)]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(MaxMethodNameLength)]
    public string? MethodName { get; set; }

    [MaxLength(MaxParametersLength)]
    public string? Parameters { get; set; }

    public DateTime ExecutionTime { get; set; }

    public int ExecutionDuration { get; set; }

    [MaxLength(MaxClientIpAddressLength)]
    public string? ClientIpAddress { get; set; }

    [MaxLength(MaxUserAgentLength)]
    public string? UserAgent { get; set; }

    public int StatusCode { get; set; }

    [MaxLength(MaxExceptionMessageLength)]
    public string? ExceptionMessage { get; set; }
}
