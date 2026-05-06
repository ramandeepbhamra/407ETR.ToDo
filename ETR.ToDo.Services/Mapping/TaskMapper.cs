using ETR.ToDo.Core.Entities.Tasks;
using ETR.ToDo.Services.Core.Tasks.Dto;
using Riok.Mapperly.Abstractions;

namespace ETR.ToDo.Services.Mapping;

/// <summary>
/// Navigation properties, FK IDs and any other source-only members are
/// silently skipped via UnmappedMemberTreatment.Ignore.
/// Sensitive infrastructure fields (soft-delete, audit) are explicitly
/// excluded below so accidental DTO additions cause a compile error.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TaskMapper
{
    [MapperIgnoreSource(nameof(TaskList.IsDeleted))]
    [MapperIgnoreSource(nameof(TaskList.DeletedDate))]
    [MapperIgnoreSource(nameof(TaskList.DeletedBy))]
    [MapperIgnoreSource(nameof(TaskList.CreatedBy))]
    [MapperIgnoreSource(nameof(TaskList.ModifiedDate))]
    [MapperIgnoreSource(nameof(TaskList.ModifiedBy))]
    public partial TaskListDto TaskListToDto(TaskList taskList);

    public partial List<TaskListDto> TaskListsToDtos(List<TaskList> taskLists);

    [MapperIgnoreSource(nameof(TodoTask.IsDeleted))]
    [MapperIgnoreSource(nameof(TodoTask.DeletedDate))]
    [MapperIgnoreSource(nameof(TodoTask.DeletedBy))]
    [MapperIgnoreSource(nameof(TodoTask.CreatedBy))]
    [MapperIgnoreSource(nameof(TodoTask.ModifiedDate))]
    [MapperIgnoreSource(nameof(TodoTask.ModifiedBy))]
    public partial TodoTaskDto TodoTaskToDto(TodoTask task);

    public partial List<TodoTaskDto> TodoTasksToDtos(List<TodoTask> tasks);

    [MapperIgnoreSource(nameof(SubTask.IsDeleted))]
    [MapperIgnoreSource(nameof(SubTask.DeletedDate))]
    [MapperIgnoreSource(nameof(SubTask.DeletedBy))]
    [MapperIgnoreSource(nameof(SubTask.CreatedBy))]
    [MapperIgnoreSource(nameof(SubTask.ModifiedDate))]
    [MapperIgnoreSource(nameof(SubTask.ModifiedBy))]
    public partial SubTaskDto SubTaskToDto(SubTask subTask);
}
