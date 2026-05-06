using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Services.Core.Authentication;
using ETR.ToDo.Services.Core.Authentication.Dto;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ETR.ToDo.Services.Authentication;

/// <summary>
/// Authentication service implementation
/// Handles user credential verification and password hashing
/// Does NOT generate JWT tokens (that's a web-layer responsibility)
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User, Guid> _userRepository;

    public AuthenticationService(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<User> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find user by email using generic repository
        var user = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        // Verify password
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Return authenticated user entity
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(userId, cancellationToken);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
