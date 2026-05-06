using ETR.ToDo.Core.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class CreateTaskListDto
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(ValidationConstants.TaskListTitle.MinLength, ErrorMessage = "Title cannot be empty")]
    [MaxLength(ValidationConstants.TaskListTitle.MaxLength, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; } = string.Empty;
}
