using ETR.ToDo.Core.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class UpdateTodoTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(ValidationConstants.TaskTitle.MinLength, ErrorMessage = "Title cannot be empty")]
    [MaxLength(ValidationConstants.TaskTitle.MaxLength, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "List is required")]
    public Guid ListId { get; set; }

    public DateOnly? DueDate { get; set; }
}
