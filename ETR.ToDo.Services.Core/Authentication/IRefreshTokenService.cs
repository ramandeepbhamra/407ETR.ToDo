using ETR.ToDo.Core.Entities.Auth;

namespace ETR.ToDo.Services.Core.Authentication;

/// <summary>
/// Service contract for refresh token lifecycle management.
/// Tokens are rotated on each use — consuming a token always produces a new one.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Creates and persists a new refresh token for the specified user.</summary>
    /// <param name="userId">The ID of the user the token is issued for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created <see cref="RefreshToken"/>.</returns>
    Task<RefreshToken> CreateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a refresh token if it exists and has not been revoked or expired.</summary>
    /// <param name="token">The raw refresh token string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching <see cref="RefreshToken"/>, or <c>null</c> if invalid.</returns>
    Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Marks a single refresh token as revoked.</summary>
    /// <param name="token">The raw refresh token string to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Revokes all active refresh tokens for a user — used when disabling an account.</summary>
    /// <param name="userId">The ID of the user whose tokens should be revoked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
