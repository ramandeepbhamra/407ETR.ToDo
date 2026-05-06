using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Users;
using ETR.ToDo.Services.Core.Users.Dto;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Full user management — Admin only.
/// Allows creating, reading, updating, and soft-deleting any user account.
/// </summary>
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Returns a paginated, filterable list of all users except the current admin.</summary>
    /// <param name="input">Pagination, sorting, and search filters.</param>
    /// <returns>A paged result of <see cref="UserDto"/>.</returns>
    /// <response code="200">Users returned successfully.</response>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersInputDto input)
    {
        input.ExcludeUserId = CurrentUserId;
        var result = await _userService.GetUsersAsync(input);
        return Success(result);
    }

    /// <summary>Returns a single user by ID.</summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The matching <see cref="UserDto"/>.</returns>
    /// <response code="200">User returned successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Success(user);
    }

    /// <summary>Creates a new user account with a specified role.</summary>
    /// <param name="input">User creation data including email, password, and role.</param>
    /// <returns>The newly created <see cref="UserDto"/>.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation failed or email is already in use.</response>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto input)
    {
        var user = await _userService.CreateUserAsync(input);
        return Created(user, "User created successfully");
    }

    /// <summary>Updates an existing user's details, role, and active status.</summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="input">Updated user data.</param>
    /// <returns>The updated <see cref="UserDto"/>.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Validation failed or email is already taken by another user.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto input)
    {
        var user = await _userService.UpdateUserAsync(id, input);
        return Success(user, "User updated successfully");
    }

    /// <summary>Soft-deletes a user account.</summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <response code="200">User deleted successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Success("User deleted successfully");
    }
}
