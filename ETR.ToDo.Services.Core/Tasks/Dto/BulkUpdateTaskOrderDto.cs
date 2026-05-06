namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class BulkUpdateTaskOrderDto
{
    public Guid ListId { get; set; }
    public List<UpdateTaskOrderDto> Updates { get; set; } = new();
}
