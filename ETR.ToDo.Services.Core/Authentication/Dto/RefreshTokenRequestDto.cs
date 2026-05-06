using System.ComponentModel.DataAnnotations;

namespace ETR.ToDo.Services.Core.Authentication.Dto;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
