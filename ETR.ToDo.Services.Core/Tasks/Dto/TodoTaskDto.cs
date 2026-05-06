namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class TodoTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ListId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsFavourite { get; set; }
    public DateOnly? DueDate { get; set; }
    public int Order { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<SubTaskDto> SubTasks { get; set; } = new();
}
