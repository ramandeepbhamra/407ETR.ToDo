namespace ETR.ToDo.Web.Core.Models;

/// <summary>
/// Non-generic API response for operations without return data
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Optional message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Creates a success response
    /// </summary>
    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse ErrorResponse(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message
        };
    }
}
