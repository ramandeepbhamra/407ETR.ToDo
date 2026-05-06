namespace ETR.ToDo.Services.Core.Authentication.Dto;

/// <summary>
/// Login response with user info and token
/// </summary>
public class LoginResponseDto
{
    public TokenResponseDto Token { get; set; } = new();
    public UserInfoDto User { get; set; } = new();
}

/// <summary>
/// User information included in login response
/// </summary>
public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
