using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Tasks.Dto;

namespace ETR.ToDo.Services.Core.Tasks;

/// <summary>
/// Service contract for task list operations.
/// All mutating methods enforce ownership: a user can only modify their own lists.
/// Basic users are subject to a list creation limit defined in app settings.
/// </summary>
public interface ITaskListService
{
    /// <summary>Returns all non-deleted task lists owned by the specified user.</summary>
    /// <param name="userId">The ID of the user whose lists to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<List<TaskListDto>> GetUserListsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new task list for the specified user.</summary>
    /// <param name="input">Task list creation data.</param>
    /// <param name="userId">The ID of the user creating the list.</param>
    /// <param name="userRole">Used to bypass the Basic-user list limit for Admin/Dev roles.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskListDto> CreateAsync(CreateTaskListDto input, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);

    /// <summary>Updates the title of an existing task list. Throws <see cref="UnauthorizedAccessException"/> if the list does not belong to the user.</summary>
    /// <param name="listId">The ID of the task list to update.</param>
    /// <param name="input">Updated task list data.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TaskListDto> UpdateAsync(Guid listId, UpdateTaskListDto input, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a task list and all its tasks.</summary>
    /// <param name="listId">The ID of the task list to delete.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid listId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Creates the default "My Tasks" list seeded for every newly registered non-admin user.</summary>
    /// <param name="userId">The ID of the newly registered user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CreateDefaultListAsync(Guid userId, CancellationToken cancellationToken = default);
}
