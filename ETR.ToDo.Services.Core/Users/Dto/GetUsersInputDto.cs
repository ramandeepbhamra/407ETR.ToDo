using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Common.Dto;

namespace ETR.ToDo.Services.Core.Users.Dto;

/// <summary>
/// Input parameters for getting users list with filtering, sorting, and pagination.
/// Supports excluding the requesting user to prevent self-management operations.
/// </summary>
public class GetUsersInputDto : PagedAndSortedRequestDto
{
    /// <summary>
    /// Gets or sets a value to filter users by active status.
    /// Null returns all users regardless of status.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value to filter system users.
    /// System users have protected roles that cannot be changed.
    /// Null returns all users regardless of system user status.
    /// </summary>
    public bool? IsSystemUser { get; set; }

    /// <summary>
    /// Gets or sets the email filter (case-insensitive partial match).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the name filter (case-insensitive partial match on FirstName or LastName).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the role filter to return only users with a specific role.
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Gets or sets a user ID to exclude from the results.
    /// Typically used to exclude the requesting user from the list.
    /// </summary>
    public Guid? ExcludeUserId { get; set; }
}
