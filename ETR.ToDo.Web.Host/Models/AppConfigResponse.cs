namespace ETR.ToDo.Web.Host.Models;

/// <summary>
/// Public app configuration served to the Angular client.
/// Contains session management settings, application info, and user limits loaded from appsettings.json.
/// </summary>
public class AppConfigResponse
{
    /// <summary>
    /// Gets or sets the application name for branding and display.
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application version for display and debugging.
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the support email for upgrade prompts and contact.
    /// </summary>
    public string SupportEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of tasks allowed for Basic users.
    /// </summary>
    public int BasicUserTaskLimit { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of task lists allowed for Basic users.
    /// </summary>
    public int BasicUserTaskListLimit { get; set; }

    /// <summary>
    /// Gets or sets the idle timeout duration in minutes before showing the warning dialog.
    /// Default value is 15 minutes if not configured in appsettings.json.
    /// </summary>
    public int IdleTimeoutMinutes { get; set; }

    /// <summary>
    /// Gets or sets the countdown duration in seconds shown in the warning dialog before automatic logout.
    /// Default value is 30 seconds if not configured in appsettings.json.
    /// </summary>
    public int WarningCountdownSeconds { get; set; }
}
