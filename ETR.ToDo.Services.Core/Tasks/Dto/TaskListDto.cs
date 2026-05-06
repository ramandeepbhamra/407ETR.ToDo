namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class TaskListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedDate { get; set; }
}
