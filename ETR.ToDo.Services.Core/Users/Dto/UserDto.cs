using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Common.Dto;

namespace ETR.ToDo.Services.Core.Users.Dto;

/// <summary>
/// User output DTO
/// </summary>
public class UserDto : EntityDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemUser { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
