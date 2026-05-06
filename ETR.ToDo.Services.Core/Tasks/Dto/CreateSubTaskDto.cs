using ETR.ToDo.Core.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Tasks.Dto;

public class CreateSubTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(ValidationConstants.TaskTitle.MinLength, ErrorMessage = "Title cannot be empty")]
    [MaxLength(ValidationConstants.TaskTitle.MaxLength, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; } = string.Empty;
}
