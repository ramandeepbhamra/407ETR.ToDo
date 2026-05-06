using ETR.ToDo.Core.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Users.Dto;

/// <summary>
/// DTO for updating the current user's own profile (FirstName, LastName, Phone only).
/// For full user updates (email, role), use UpdateUserDto via UsersController (Admin only).
/// </summary>
public class UpdateProfileDto
{
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
}
