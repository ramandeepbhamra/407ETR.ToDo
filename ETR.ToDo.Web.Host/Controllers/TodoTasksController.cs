using ETR.ToDo.Services.Core.Tasks;
using ETR.ToDo.Services.Core.Tasks.Dto;
using ETR.ToDo.Web.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETR.ToDo.Web.Host.Controllers;

/// <summary>
/// Task and subtask management — scoped to the authenticated user's own tasks.
/// Basic users are subject to a per-list task limit enforced at the service layer.
/// Subtask routes are nested under their parent task: /api/tasks/{id}/subtasks.
/// </summary>
[Authorize]
[Route("api/tasks")]
public class TodoTasksController : ApiControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService;
    }

    /// <summary>Returns all tasks in a given task list.</summary>
    /// <param name="listId">The ID of the task list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="TodoTaskDto"/> in the specified list.</returns>
    /// <response code="200">Tasks returned successfully.</response>
    /// <response code="403">The task list does not belong to the current user.</response>
    [HttpGet]
    public async Task<IActionResult> GetByList([FromQuery] Guid listId, CancellationToken cancellationToken)
    {
        var tasks = await _todoTaskService.GetByListAsync(listId, CurrentUserId, cancellationToken);
        return Success(tasks);
    }

    /// <summary>Returns all tasks marked as favourite by the current user, across all lists.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="TodoTaskDto"/> marked as favourite.</returns>
    /// <response code="200">Favourite tasks returned successfully.</response>
    [HttpGet("favourites")]
    public async Task<IActionResult> GetFavourites(CancellationToken cancellationToken)
    {
        var tasks = await _todoTaskService.GetFavouritesAsync(CurrentUserId, cancellationToken);
        return Success(tasks);
    }

    /// <summary>Creates a new task in the specified list.</summary>
    /// <param name="input">Task creation data including the target list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created <see cref="TodoTaskDto"/>.</returns>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Basic users have reached their task limit, or input is invalid.</response>
    /// <response code="403">The task list does not belong to the current user.</response>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoTaskDto input, CancellationToken cancellationToken)
    {
        var task = await _todoTaskService.CreateAsync(input, CurrentUserId, CurrentUserRole, cancellationToken);
        return Created(task, "Task created");
    }

    /// <summary>Updates an existing task's details.</summary>
    /// <param name="id">The ID of the task to update.</param>
    /// <param name="input">Updated task data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TodoTaskDto"/>.</returns>
    /// <response code="200">Task updated successfully.</response>
    /// <response code="403">The task does not belong to the current user.</response>
    /// <response code="404">Task not found.</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoTaskDto input, CancellationToken cancellationToken)
    {
        var task = await _todoTaskService.UpdateAsync(id, input, CurrentUserId, cancellationToken);
        return Success(task, "Task updated");
    }

    /// <summary>Soft-deletes a task and all its subtasks.</summary>
    /// <param name="id">The ID of the task to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Task deleted successfully.</response>
    /// <response code="403">The task does not belong to the current user.</response>
    /// <response code="404">Task not found.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _todoTaskService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Success("Task deleted");
    }

    /// <summary>Toggles the completed state of a task.</summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TodoTaskDto"/> with the new completion state.</returns>
    /// <response code="200">Completion state toggled successfully.</response>
    /// <response code="403">The task does not belong to the current user.</response>
    /// <response code="404">Task not found.</response>
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> ToggleComplete(Guid id, CancellationToken cancellationToken)
    {
        var task = await _todoTaskService.ToggleCompleteAsync(id, CurrentUserId, CurrentUserRole, cancellationToken);
        return Success(task);
    }

    /// <summary>Toggles the favourite state of a task.</summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TodoTaskDto"/> with the new favourite state.</returns>
    /// <response code="200">Favourite state toggled successfully.</response>
    /// <response code="403">The task does not belong to the current user.</response>
    /// <response code="404">Task not found.</response>
    [HttpPatch("{id:guid}/favourite")]
    public async Task<IActionResult> ToggleFavourite(Guid id, CancellationToken cancellationToken)
    {
        var task = await _todoTaskService.ToggleFavouriteAsync(id, CurrentUserId, cancellationToken);
        return Success(task);
    }

    /// <summary>Adds a subtask to an existing task.</summary>
    /// <param name="id">The ID of the parent task.</param>
    /// <param name="input">Subtask creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created <see cref="SubTaskDto"/>.</returns>
    /// <response code="201">Subtask added successfully.</response>
    /// <response code="403">The parent task does not belong to the current user.</response>
    /// <response code="404">Parent task not found.</response>
    [HttpPost("{id:guid}/subtasks")]
    public async Task<IActionResult> AddSubTask(Guid id, [FromBody] CreateSubTaskDto input, CancellationToken cancellationToken)
    {
        var subTask = await _todoTaskService.AddSubTaskAsync(id, input, CurrentUserId, cancellationToken);
        return Created(subTask, "Subtask added");
    }

    /// <summary>Updates an existing subtask's title.</summary>
    /// <param name="taskId">The ID of the parent task.</param>
    /// <param name="subTaskId">The ID of the subtask to update.</param>
    /// <param name="input">Updated subtask data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="SubTaskDto"/>.</returns>
    /// <response code="200">Subtask updated successfully.</response>
    /// <response code="403">The subtask does not belong to the current user.</response>
    /// <response code="404">Subtask not found.</response>
    [HttpPut("{taskId:guid}/subtasks/{subTaskId:guid}")]
    public async Task<IActionResult> UpdateSubTask(Guid taskId, Guid subTaskId, [FromBody] CreateSubTaskDto input, CancellationToken cancellationToken)
    {
        var subTask = await _todoTaskService.UpdateSubTaskAsync(subTaskId, input, CurrentUserId, cancellationToken);
        return Success(subTask, "Subtask updated");
    }

    /// <summary>Toggles the completed state of a subtask.</summary>
    /// <param name="taskId">The ID of the parent task.</param>
    /// <param name="subTaskId">The ID of the subtask.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="SubTaskDto"/> with the new completion state.</returns>
    /// <response code="200">Subtask completion state toggled successfully.</response>
    /// <response code="403">The subtask does not belong to the current user.</response>
    /// <response code="404">Subtask not found.</response>
    [HttpPatch("{taskId:guid}/subtasks/{subTaskId:guid}/complete")]
    public async Task<IActionResult> ToggleSubTaskComplete(Guid taskId, Guid subTaskId, CancellationToken cancellationToken)
    {
        var subTask = await _todoTaskService.ToggleSubTaskCompleteAsync(subTaskId, CurrentUserId, cancellationToken);
        return Success(subTask);
    }

    /// <summary>Soft-deletes a subtask.</summary>
    /// <param name="taskId">The ID of the parent task.</param>
    /// <param name="subTaskId">The ID of the subtask to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Subtask deleted successfully.</response>
    /// <response code="403">The subtask does not belong to the current user.</response>
    /// <response code="404">Subtask not found.</response>
    [HttpDelete("{taskId:guid}/subtasks/{subTaskId:guid}")]
    public async Task<IActionResult> DeleteSubTask(Guid taskId, Guid subTaskId, CancellationToken cancellationToken)
    {
        await _todoTaskService.DeleteSubTaskAsync(subTaskId, CurrentUserId, cancellationToken);
        return Success("Subtask deleted");
    }

    /// <summary>Bulk-updates the display order of tasks within a list.</summary>
    /// <param name="input">List of task IDs with their new ordinal positions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Task order updated successfully.</response>
    /// <response code="403">One or more tasks do not belong to the current user.</response>
    [HttpPut("update-order")]
    public async Task<IActionResult> UpdateOrder([FromBody] BulkUpdateTaskOrderDto input, CancellationToken cancellationToken)
    {
        await _todoTaskService.UpdateOrderAsync(input, CurrentUserId, cancellationToken);
        return Success("Task order updated");
    }
}
