using ETR.ToDo.Core.Entities.Tasks;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Core.Shared.Configuration;
using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Tasks;
using ETR.ToDo.Services.Core.Tasks.Dto;
using ETR.ToDo.Services.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ETR.ToDo.Services.Tasks;

public class TodoTaskService : ITodoTaskService
{
    private readonly IRepository<TodoTask, Guid> _taskRepository;
    private readonly IRepository<SubTask, Guid> _subTaskRepository;
    private readonly IRepository<TaskList, Guid> _listRepository;
    private readonly TaskMapper _mapper;
    private readonly ApplicationSettings _appSettings;

    public TodoTaskService(
        IRepository<TodoTask, Guid> taskRepository,
        IRepository<SubTask, Guid> subTaskRepository,
        IRepository<TaskList, Guid> listRepository,
        TaskMapper mapper,
        IOptions<ApplicationSettings> appSettings)
    {
        _taskRepository = taskRepository;
        _subTaskRepository = subTaskRepository;
        _listRepository = listRepository;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public async Task<List<TodoTaskDto>> GetByListAsync(Guid listId, Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _listRepository.GetAll()
            .FirstOrDefaultAsync(tl => tl.Id == listId && tl.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Task list not found.");

        var tasks = await _taskRepository
            .GetAll()
            .Where(t => t.ListId == listId && t.UserId == userId)
            .Include(t => t.SubTasks)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        return _mapper.TodoTasksToDtos(tasks);
    }

    public async Task<List<TodoTaskDto>> GetFavouritesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository
            .GetAll()
            .Where(t => t.UserId == userId && t.IsFavourite)
            .Include(t => t.SubTasks)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        return _mapper.TodoTasksToDtos(tasks);
    }

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto input, Guid userId, UserRole userRole, CancellationToken cancellationToken = default)
    {
        var list = await _listRepository.GetAll()
            .FirstOrDefaultAsync(tl => tl.Id == input.ListId && tl.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Task list not found.");

        if (userRole == UserRole.Basic)
        {
            var taskCountInList = await GetTaskCountInListAsync(input.ListId, cancellationToken);
            if (taskCountInList >= _appSettings.Limits.BasicUserTaskLimit)
                throw new InvalidOperationException(
                    $"You have reached the limit of {_appSettings.Limits.BasicUserTaskLimit} tasks per list. Upgrade to Premium for unlimited tasks.");
        }

        var order = await _taskRepository
            .GetAll()
            .Where(t => t.ListId == input.ListId)
            .CountAsync(cancellationToken) + 1;

        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            ListId = input.ListId,
            UserId = userId,
            DueDate = input.DueDate,
            Order = order
        };

        var created = await _taskRepository.InsertAsync(task, cancellationToken);
        created.SubTasks = new List<SubTask>();
        return _mapper.TodoTaskToDto(created);
    }

    public async Task<TodoTaskDto> UpdateAsync(Guid taskId, UpdateTodoTaskDto input, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetAll()
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        if (input.ListId != task.ListId)
        {
            var newList = await _listRepository.GetAll()
                .FirstOrDefaultAsync(tl => tl.Id == input.ListId && tl.UserId == userId, cancellationToken)
                ?? throw new KeyNotFoundException("Target task list not found.");

            task.ListId = newList.Id;
        }

        task.Title = input.Title;
        task.DueDate = input.DueDate;

        var updated = await _taskRepository.UpdateAsync(task, cancellationToken);
        return _mapper.TodoTaskToDto(updated);
    }

    public async Task DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetAll()
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        foreach (var subTask in task.SubTasks)
            await _subTaskRepository.DeleteAsync(subTask.Id, cancellationToken);

        await _taskRepository.DeleteAsync(taskId, cancellationToken);
    }

    public async Task<TodoTaskDto> ToggleCompleteAsync(Guid taskId, Guid userId, UserRole userRole, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetAll()
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        // Store the new completion state
        var willBeCompleted = !task.IsCompleted;

        task.IsCompleted = willBeCompleted;
        task.CompletedDate = task.IsCompleted ? DateTime.UtcNow : null;

        // Cascade completion ONLY when marking task as complete
        // When marking as incomplete, keep subtask states as-is
        if (willBeCompleted)
        {
            foreach (var subTask in task.SubTasks)
            {
                subTask.IsCompleted = true;
            }
        }

        var updated = await _taskRepository.UpdateAsync(task, cancellationToken);
        return _mapper.TodoTaskToDto(updated);
    }

    public async Task<TodoTaskDto> ToggleFavouriteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetAll()
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        task.IsFavourite = !task.IsFavourite;

        var updated = await _taskRepository.UpdateAsync(task, cancellationToken);
        return _mapper.TodoTaskToDto(updated);
    }

    public async Task<SubTaskDto> AddSubTaskAsync(Guid taskId, CreateSubTaskDto input, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetAll()
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        if (task.IsCompleted)
            throw new InvalidOperationException("Cannot add subtasks to a completed task. Please mark the task as incomplete first.");

        var subTask = new SubTask
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            TodoTaskId = taskId
        };

        var created = await _subTaskRepository.InsertAsync(subTask, cancellationToken);
        return _mapper.SubTaskToDto(created);
    }

    public async Task<SubTaskDto> UpdateSubTaskAsync(Guid subTaskId, CreateSubTaskDto input, Guid userId, CancellationToken cancellationToken = default)
    {
        var subTask = await _subTaskRepository.GetAll()
            .Include(st => st.TodoTask)
            .FirstOrDefaultAsync(st => st.Id == subTaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subtask {subTaskId} not found.");

        if (subTask.TodoTask.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this subtask.");

        subTask.Title = input.Title;
        var updated = await _subTaskRepository.UpdateAsync(subTask, cancellationToken);
        return _mapper.SubTaskToDto(updated);
    }

    public async Task<SubTaskDto> ToggleSubTaskCompleteAsync(Guid subTaskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var subTask = await _subTaskRepository.GetAll()
            .Include(st => st.TodoTask)
            .FirstOrDefaultAsync(st => st.Id == subTaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subtask {subTaskId} not found.");

        if (subTask.TodoTask.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this subtask.");

        subTask.IsCompleted = !subTask.IsCompleted;
        var updated = await _subTaskRepository.UpdateAsync(subTask, cancellationToken);
        return _mapper.SubTaskToDto(updated);
    }

    public async Task DeleteSubTaskAsync(Guid subTaskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var subTask = await _subTaskRepository.GetAll()
            .Include(st => st.TodoTask)
            .FirstOrDefaultAsync(st => st.Id == subTaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subtask {subTaskId} not found.");

        if (subTask.TodoTask.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this subtask.");

        await _subTaskRepository.DeleteAsync(subTaskId, cancellationToken);
    }

    private Task<int> GetTaskCountInListAsync(Guid listId, CancellationToken cancellationToken)
        => _taskRepository
            .GetAll()
            .CountAsync(t => t.ListId == listId && !t.IsDeleted, cancellationToken);

    public async Task UpdateOrderAsync(BulkUpdateTaskOrderDto input, Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _listRepository.GetAll()
            .FirstOrDefaultAsync(tl => tl.Id == input.ListId && tl.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Task list not found.");

        foreach (var update in input.Updates)
        {
            var task = await _taskRepository.GetAll()
                .FirstOrDefaultAsync(t => t.Id == update.Id && t.UserId == userId, cancellationToken);

            if (task != null && task.ListId == input.ListId)
            {
                task.Order = update.Order;
                await _taskRepository.UpdateAsync(task, cancellationToken);
            }
        }
    }
}
