using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Tasks.Dto;

namespace ETR.ToDo.Services.Core.Tasks;

/// <summary>
/// Service contract for task and subtask operations.
/// All mutating methods enforce ownership: a user can only modify their own tasks.
/// Basic users are subject to a per-list task creation limit defined in app settings.
/// </summary>
public interface ITodoTaskService
{
    /// <summary>Returns all non-deleted tasks in a list, scoped to the requesting user.</summary>
    /// <param name="listId">The ID of the task list.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<List<TodoTaskDto>> GetByListAsync(Guid listId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns all tasks marked as favourite by the user, across all their lists.</summary>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<List<TodoTaskDto>> GetFavouritesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new task in the specified list.</summary>
    /// <param name="input">Task creation data including the target list ID.</param>
    /// <param name="userId">The ID of the user creating the task.</param>
    /// <param name="userRole">Used to bypass the Basic-user task limit for Admin/Dev roles.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto input, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing task's details. Throws <see cref="UnauthorizedAccessException"/> if the task does not belong to the user.</summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="input">Updated task data.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TodoTaskDto> UpdateAsync(Guid taskId, UpdateTodoTaskDto input, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a task and all its subtasks.</summary>
    /// <param name="taskId">The ID of the task to delete.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Toggles the <c>IsCompleted</c> flag on a task.</summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="userRole">Used to allow Dev role to complete any task.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TodoTaskDto> ToggleCompleteAsync(Guid taskId, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>Toggles the <c>IsFavourite</c> flag on a task.</summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TodoTaskDto> ToggleFavouriteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new subtask to an existing task.</summary>
    /// <param name="taskId">The ID of the parent task.</param>
    /// <param name="input">Subtask creation data.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SubTaskDto> AddSubTaskAsync(Guid taskId, CreateSubTaskDto input, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing subtask's title.</summary>
    /// <param name="subTaskId">The ID of the subtask to update.</param>
    /// <param name="input">Updated subtask data.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SubTaskDto> UpdateSubTaskAsync(Guid subTaskId, CreateSubTaskDto input, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Toggles the <c>IsCompleted</c> flag on a subtask.</summary>
    /// <param name="subTaskId">The ID of the subtask.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SubTaskDto> ToggleSubTaskCompleteAsync(Guid subTaskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a subtask.</summary>
    /// <param name="subTaskId">The ID of the subtask to delete.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteSubTaskAsync(Guid subTaskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Bulk-updates the display order of tasks within a list.</summary>
    /// <param name="input">Collection of task IDs with their new ordinal positions.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateOrderAsync(BulkUpdateTaskOrderDto input, Guid userId, CancellationToken cancellationToken = default);
}
