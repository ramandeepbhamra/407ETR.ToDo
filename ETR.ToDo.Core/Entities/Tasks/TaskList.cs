using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ETR.ToDo.Core.Entities.Base;
using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Shared.Constants;

namespace ETR.ToDo.Core.Entities.Tasks;

[Table("TaskLists")]
public class TaskList : BaseEntity<Guid>
{
    [Required]
    [MinLength(ValidationConstants.TaskListTitle.MinLength)]
    [MaxLength(ValidationConstants.TaskListTitle.MaxLength)]
    public string Title { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public int Order { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
}
