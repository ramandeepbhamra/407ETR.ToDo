using System.Security.Cryptography;
using ETR.ToDo.Core.Entities.Auth;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Services.Core.Authentication;
using Microsoft.EntityFrameworkCore;

namespace ETR.ToDo.Services.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRepository<RefreshToken, Guid> _repository;
    private const int ExpiryDays = 7;
    private const int TokenByteSize = 64;

    public RefreshTokenService(IRepository<RefreshToken, Guid> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RefreshToken> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpiryDays),
            IsRevoked = false
        };

        return await _repository.InsertAsync(token, cancellationToken);
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _repository
            .GetAll()
            .FirstOrDefaultAsync(
                rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var existing = await _repository
            .GetAll()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (existing is null) return;

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _repository
            .GetAll()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(token, cancellationToken);
        }
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[TokenByteSize];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
