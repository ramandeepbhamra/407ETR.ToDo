namespace ETR.ToDo.Services.Core.Authentication.Dto;

/// <summary>
/// JWT token information
/// </summary>
public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
