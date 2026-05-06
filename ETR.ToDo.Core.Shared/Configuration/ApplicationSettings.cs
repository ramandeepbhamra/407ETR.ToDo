namespace ETR.ToDo.Core.Shared.Configuration;

/// <summary>
/// Application-wide configuration settings
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Application name for branding and display
    /// </summary>
    public string Name { get; set; } = "ETR.ToDo";

    /// <summary>
    /// Application version for display and debugging
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Support email for upgrade prompts and contact
    /// </summary>
    public string SupportEmail { get; set; } = "support@functionhealth.com";

    /// <summary>
    /// User limits configuration
    /// </summary>
    public LimitsSettings Limits { get; set; } = new();
}
