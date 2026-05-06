using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Authentication.Dto;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
