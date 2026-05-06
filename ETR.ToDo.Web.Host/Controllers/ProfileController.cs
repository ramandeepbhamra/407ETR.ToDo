using ETR.ToDo.Services.Core.Users;
using ETR.ToDo.Services.Core.Users.Dto;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Profile controller — allows any authenticated user to manage their own profile.
/// Users can view and update their FirstName, LastName, and PhoneNumber only.
/// Email, role, and password changes require admin access via UsersController.
/// </summary>
[Authorize]
public class ProfileController : ApiControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets the current user's profile information.
    /// </summary>
    /// <returns>A <see cref="UserDto"/> containing the current user's profile data.</returns>
    /// <response code="200">Returns the user's profile successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetUserByIdAsync(CurrentUserId);
        return Success(user);
    }

    /// <summary>
    /// Updates the current user's own profile (FirstName, LastName, Phone only).
    /// This endpoint only allows editing basic profile fields.
    /// To change email, role, or password, use the admin UsersController.
    /// </summary>
    /// <param name="input">The profile update data.</param>
    /// <returns>The updated <see cref="UserDto"/>.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="404">User not found.</response>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto input)
    {
        var user = await _userService.UpdateProfileAsync(CurrentUserId, input);
        return Success(user, "Profile updated successfully");
    }
}
