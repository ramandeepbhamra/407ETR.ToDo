namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class SubTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
}
