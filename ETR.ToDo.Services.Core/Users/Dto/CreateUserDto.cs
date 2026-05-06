using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Core.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Users.Dto;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(ValidationConstants.Email.MaxLength, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(ValidationConstants.Password.MinLength, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(ValidationConstants.Password.MaxLength, ErrorMessage = "Password cannot exceed 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [MinLength(ValidationConstants.UserName.MinLength, ErrorMessage = "First name cannot be empty")]
    [MaxLength(ValidationConstants.UserName.MaxLength, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MinLength(ValidationConstants.UserName.MinLength, ErrorMessage = "Last name cannot be empty")]
    [MaxLength(ValidationConstants.UserName.MaxLength, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(ValidationConstants.PhoneNumber.MaxLength, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }
}
