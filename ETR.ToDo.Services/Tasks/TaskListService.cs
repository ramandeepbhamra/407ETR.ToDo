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

public class TaskListService : ITaskListService
{
    private readonly IRepository<TaskList, Guid> _listRepository;
    private readonly IRepository<TodoTask, Guid> _taskRepository;
    private readonly IRepository<SubTask, Guid> _subTaskRepository;
    private readonly TaskMapper _mapper;
    private readonly ApplicationSettings _appSettings;

    public TaskListService(
        IRepository<TaskList, Guid> listRepository,
        IRepository<TodoTask, Guid> taskRepository,
        IRepository<SubTask, Guid> subTaskRepository,
        TaskMapper mapper,
        IOptions<ApplicationSettings> appSettings)
    {
        _listRepository = listRepository;
        _taskRepository = taskRepository;
        _subTaskRepository = subTaskRepository;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public async Task<List<TaskListDto>> GetUserListsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var lists = await _listRepository
            .GetAll()
            .Where(tl => tl.UserId == userId)
            .OrderBy(tl => tl.Order)
            .ToListAsync(cancellationToken);

        return _mapper.TaskListsToDtos(lists);
    }

    public async Task<TaskListDto> CreateAsync(CreateTaskListDto input, Guid userId, UserRole userRole, CancellationToken cancellationToken = default)
    {
        // Enforce task list limit for Basic users
        if (userRole == UserRole.Basic)
        {
            var activeListCount = await _listRepository
                .GetAll()
                .Where(tl => tl.UserId == userId && !tl.IsDeleted)
                .CountAsync(cancellationToken);

            if (activeListCount >= _appSettings.Limits.BasicUserTaskListLimit)
                throw new InvalidOperationException(
                    $"You have reached the limit of {_appSettings.Limits.BasicUserTaskListLimit} task lists. Upgrade to Premium for unlimited task lists.");
        }

        var order = await _listRepository
            .GetAll()
            .Where(tl => tl.UserId == userId)
            .CountAsync(cancellationToken) + 1;

        var list = new TaskList
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            UserId = userId,
            Order = order
        };

        var created = await _listRepository.InsertAsync(list, cancellationToken);
        return _mapper.TaskListToDto(created);
    }

    public async Task<TaskListDto> UpdateAsync(Guid listId, UpdateTaskListDto input, Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _listRepository
            .GetAll()
            .FirstOrDefaultAsync(tl => tl.Id == listId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task list {listId} not found.");

        if (list.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this list.");

        list.Title = input.Title;
        var updated = await _listRepository.UpdateAsync(list, cancellationToken);
        return _mapper.TaskListToDto(updated);
    }

    public async Task DeleteAsync(Guid listId, Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _listRepository
            .GetAll()
            .Include(tl => tl.Tasks)
                .ThenInclude(t => t.SubTasks)
            .FirstOrDefaultAsync(tl => tl.Id == listId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task list {listId} not found.");

        if (list.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this list.");

        // Use transaction to ensure atomic cascade delete
        await using var transaction = await _listRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            // Cascade delete: SubTasks → Tasks → TaskList
            foreach (var task in list.Tasks)
            {
                foreach (var subTask in task.SubTasks)
                    await _subTaskRepository.DeleteAsync(subTask.Id, cancellationToken);

                await _taskRepository.DeleteAsync(task.Id, cancellationToken);
            }

            await _listRepository.DeleteAsync(listId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task CreateDefaultListAsync(Guid userId, CancellationToken cancellationToken = default)
        => CreateAsync(new CreateTaskListDto { Title = "My Tasks" }, userId, UserRole.Basic, cancellationToken);
}
