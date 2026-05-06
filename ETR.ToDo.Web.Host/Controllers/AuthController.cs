using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Authentication;
using ETR.ToDo.Services.Core.Authentication.Dto;
using ETR.ToDo.Services.Core.Tasks;
using ETR.ToDo.Services.Core.Users;
using ETR.ToDo.Services.Core.Users.Dto;
using ETR.ToDo.Web.Core.Authentication;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Authentication — login, self-registration, token refresh, and revocation.
/// All endpoints except <c>revoke</c> are accessible without a prior access token.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserService _userService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITaskListService _taskListService;

    public AuthController(
        IAuthenticationService authenticationService,
        IJwtTokenService jwtTokenService,
        IUserService userService,
        IRefreshTokenService refreshTokenService,
        ITaskListService taskListService)
    {
        _authenticationService = authenticationService;
        _jwtTokenService = jwtTokenService;
        _userService = userService;
        _refreshTokenService = refreshTokenService;
        _taskListService = taskListService;
    }

    /// <summary>Authenticates a user with email and password.</summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="LoginResponseDto"/> containing the access token, refresh token, and user info.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _authenticationService.AuthenticateAsync(request, cancellationToken);
        var token = await BuildFullTokenAsync(user, cancellationToken);

        return Success(BuildLoginResponse(user, token), "Login successful");
    }

    /// <summary>Registers a new Basic user account and auto-authenticates on success.</summary>
    /// <param name="request">New account details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="LoginResponseDto"/> identical to a successful login response.</returns>
    /// <response code="201">Registration successful — returns tokens and user info.</response>
    /// <response code="400">Validation failed or email is already in use.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request, CancellationToken cancellationToken)
    {
        await _userService.CreateUserAsync(request, cancellationToken);

        var loginRequest = new LoginRequestDto { Email = request.Email, Password = request.Password };
        var user = await _authenticationService.AuthenticateAsync(loginRequest, cancellationToken);

        if (user.Role != UserRole.Admin)
            await _taskListService.CreateDefaultListAsync(user.Id, cancellationToken);

        var token = await BuildFullTokenAsync(user, cancellationToken);

        return Created(BuildLoginResponse(user, token), "Registration successful");
    }

    /// <summary>Exchanges a valid refresh token for a new access token and a rotated refresh token.</summary>
    /// <param name="request">The current refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="LoginResponseDto"/> with fresh tokens.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="401">Refresh token is invalid, expired, or the user account is inactive.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenService.GetValidTokenAsync(request.RefreshToken, cancellationToken);
        if (existing is null)
            return Unauthorized("Refresh token is invalid or has expired.");

        var user = await _authenticationService.GetUserByIdAsync(existing.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Unauthorized("User account is inactive or does not exist.");

        await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        var token = await BuildFullTokenAsync(user, cancellationToken);

        return Success(BuildLoginResponse(user, token), "Token refreshed");
    }

    /// <summary>Revokes a refresh token, logging out the current device.</summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Logged out successfully.</response>
    /// <response code="401">Access token missing or invalid.</response>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return Success("Logged out successfully");
    }

    private async Task<TokenResponseDto> BuildFullTokenAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerateToken(user);
        var refreshToken = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);
        accessToken.RefreshToken = refreshToken.Token;
        accessToken.RefreshTokenExpiresAt = refreshToken.ExpiresAt;
        return accessToken;
    }

    private static LoginResponseDto BuildLoginResponse(User user, TokenResponseDto token) =>
        new()
        {
            Token = token,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            }
        };
}
