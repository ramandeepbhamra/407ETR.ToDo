namespace ETR.ToDo.Web.Core.Models;

/// <summary>
/// Standardized error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Always false for error responses
    /// </summary>
    public bool Success => false;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (optional, for debugging)
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Validation errors (if applicable)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
