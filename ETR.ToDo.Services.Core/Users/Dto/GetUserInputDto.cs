namespace ETR.ToDo.Services.Core.Users.Dto;

/// <summary>
/// Simple input for filtering users (backward compatibility)
/// </summary>
public class GetUserInputDto
{
    public string? Filter { get; set; }
}
