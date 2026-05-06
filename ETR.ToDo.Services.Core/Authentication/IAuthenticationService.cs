using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Services.Core.Authentication.Dto;

namespace ETR.ToDo.Services.Core.Authentication;

/// <summary>
/// Authentication service interface
/// Handles user credential verification only (no JWT generation)
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user by email and password
    /// Returns User entity if successful, throws exception if failed
    /// </summary>
    Task<User> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user entity by ID — used by the refresh token flow
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a password against a stored hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    string HashPassword(string password);
}
