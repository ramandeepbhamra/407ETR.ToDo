using System.Security.Claims;
using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirst(JwtClaimTypes.UserId)?.Value;
            return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("User ID claim is missing.");
        }
    }

    protected UserRole CurrentUserRole
    {
        get
        {
            var value = User.FindFirst(JwtClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(value, ignoreCase: true, out var role) ? role : throw new UnauthorizedAccessException("Role claim is missing.");
        }
    }

    protected IActionResult Success<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.SuccessResponse(data, message));

    protected IActionResult Success(string? message = null)
        => Ok(ApiResponse.SuccessResponse(message));

    protected IActionResult Created<T>(T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.SuccessResponse(data, message));

    protected IActionResult BadRequest(string message)
        => StatusCode(400, ApiResponse.ErrorResponse(message));

    protected IActionResult NotFound(string message)
        => StatusCode(404, ApiResponse.ErrorResponse(message));

    protected IActionResult Unauthorized(string message)
        => StatusCode(401, ApiResponse.ErrorResponse(message));

    protected IActionResult Forbidden(string message)
        => StatusCode(403, ApiResponse.ErrorResponse(message));
}
