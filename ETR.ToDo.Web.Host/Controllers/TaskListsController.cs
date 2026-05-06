using ETR.ToDo.Services.Core.Tasks;
using ETR.ToDo.Services.Core.Tasks.Dto;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Task list management — scoped to the authenticated user's own lists.
/// Admins and Dev users bypass the per-user list limit enforced at the service layer.
/// </summary>
[Authorize]
[Route("api/tasklists")]
public class TaskListsController : ApiControllerBase
{
    private readonly ITaskListService _taskListService;

    public TaskListsController(ITaskListService taskListService)
    {
        _taskListService = taskListService;
    }

    /// <summary>Returns all task lists owned by the current user.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="TaskListDto"/> belonging to the current user.</returns>
    /// <response code="200">Lists returned successfully.</response>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var lists = await _taskListService.GetUserListsAsync(CurrentUserId, cancellationToken);
        return Success(lists);
    }

    /// <summary>Creates a new task list for the current user.</summary>
    /// <param name="input">Task list creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created <see cref="TaskListDto"/>.</returns>
    /// <response code="201">List created successfully.</response>
    /// <response code="400">Basic users have reached their list limit, or input is invalid.</response>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskListDto input, CancellationToken cancellationToken)
    {
        var list = await _taskListService.CreateAsync(input, CurrentUserId, CurrentUserRole, cancellationToken);
        return Created(list, "List created");
    }

    /// <summary>Updates the title of an existing task list.</summary>
    /// <param name="id">The ID of the task list to update.</param>
    /// <param name="input">Updated task list data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TaskListDto"/>.</returns>
    /// <response code="200">List updated successfully.</response>
    /// <response code="403">The list does not belong to the current user.</response>
    /// <response code="404">Task list not found.</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskListDto input, CancellationToken cancellationToken)
    {
        var list = await _taskListService.UpdateAsync(id, input, CurrentUserId, cancellationToken);
        return Success(list, "List updated");
    }

    /// <summary>Soft-deletes a task list and all its tasks.</summary>
    /// <param name="id">The ID of the task list to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">List deleted successfully.</response>
    /// <response code="403">The list does not belong to the current user.</response>
    /// <response code="404">Task list not found.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskListService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Success("List deleted");
    }
}
