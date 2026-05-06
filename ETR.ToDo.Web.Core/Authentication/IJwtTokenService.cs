using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Services.Core.Authentication.Dto;

namespace ETR.ToDo.Web.Core.Authentication;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>Token response with access token and expiration</returns>
    TokenResponseDto GenerateToken(User user);
}
