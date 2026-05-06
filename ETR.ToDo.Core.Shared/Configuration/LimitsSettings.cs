namespace ETR.ToDo.Core.Shared.Configuration;

/// <summary>
/// User limits configuration for different subscription tiers
/// </summary>
public class LimitsSettings
{
    /// <summary>
    /// Maximum number of tasks allowed for Basic users
    /// </summary>
    public int BasicUserTaskLimit { get; set; } = 10;

    /// <summary>
    /// Maximum number of task lists allowed for Basic users
    /// </summary>
    public int BasicUserTaskListLimit { get; set; } = 10;
}
