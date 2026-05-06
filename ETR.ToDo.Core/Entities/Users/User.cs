using ETR.ToDo.Core.Entities.Base;
using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Core.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETR.ToDo.Core.Entities.Users;

[Table("Users")]
public class User : BaseEntity<Guid>
{
    [Required]
    [MaxLength(ValidationConstants.Email.MaxLength)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.PasswordHash.MaxLength)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.UserName.MaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.UserName.MaxLength)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.PhoneNumber.MaxLength)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsSystemUser { get; set; } = false;

    public UserRole Role { get; set; } = UserRole.Basic;

    /// <summary>
    /// Gets the user's full name
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
