using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ETR.ToDo.Core.Entities.Base;
using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Shared.Constants;

namespace ETR.ToDo.Core.Entities.Tasks;

[Table("TodoTasks")]
public class TodoTask : BaseEntity<Guid>
{
    [Required]
    [MinLength(ValidationConstants.TaskTitle.MinLength)]
    [MaxLength(ValidationConstants.TaskTitle.MaxLength)]
    public string Title { get; set; } = string.Empty;

    public Guid ListId { get; set; }

    public Guid UserId { get; set; }

    public bool IsCompleted { get; set; }

    /// <summary>UTC timestamp of when the task was completed</summary>
    public DateTime? CompletedDate { get; set; }

    public bool IsFavourite { get; set; }

    /// <summary>Date only — no time component. Stored as date in SQL Server.</summary>
    public DateOnly? DueDate { get; set; }

    public int Order { get; set; }

    public virtual TaskList List { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
}
