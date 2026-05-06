using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ETR.ToDo.Core.Entities.Base;
using ETR.ToDo.Core.Shared.Constants;

namespace ETR.ToDo.Core.Entities.Tasks;

/// <summary>
/// One-level-deep subtask. No due date, no favourite, no further nesting.
/// </summary>
[Table("SubTasks")]
public class SubTask : BaseEntity<Guid>
{
    [Required]
    [MinLength(ValidationConstants.TaskTitle.MinLength)]
    [MaxLength(ValidationConstants.TaskTitle.MaxLength)]
    public string Title { get; set; } = string.Empty;

    public Guid TodoTaskId { get; set; }

    public bool IsCompleted { get; set; }

    public virtual TodoTask TodoTask { get; set; } = null!;
}
