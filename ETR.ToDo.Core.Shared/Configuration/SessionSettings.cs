namespace ETR.ToDo.Core.Shared.Configuration;

/// <summary>
/// Session management configuration settings
/// </summary>
public class SessionSettings
{
    /// <summary>
    /// Idle timeout duration in minutes before user is logged out
    /// </summary>
    public int IdleTimeoutMinutes { get; set; } = 15;

    /// <summary>
    /// Warning countdown duration in seconds before automatic logout
    /// </summary>
    public int WarningCountdownSeconds { get; set; } = 30;
}
