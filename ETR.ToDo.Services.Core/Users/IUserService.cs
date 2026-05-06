using ETR.ToDo.Services.Core.Common.Dto;
using ETR.ToDo.Services.Core.Users.Dto;

namespace ETR.ToDo.Services.Core.Users;

/// <summary>
/// User service interface for all user-related operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a paginated list of users with filtering and sorting
    /// </summary>
    Task<PagedResultDto<UserDto>> GetUsersAsync(GetUsersInputDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user
    /// </summary>
    Task<UserDto> CreateUserAsync(CreateUserDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current user's own profile (FirstName, LastName, Phone only)
    /// </summary>
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user (soft delete)
    /// </summary>
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets simple user list (lightweight - existing method)
    /// </summary>
    Task<List<UserListDto>> GetUser(GetUserInputDto input);
}
