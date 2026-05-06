using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Users.Dto;

public class UserListDto
{
    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}
