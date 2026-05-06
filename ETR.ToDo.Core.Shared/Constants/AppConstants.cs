namespace ETR.ToDo.Core.Shared.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Application name
    /// </summary>
    public const string ApplicationName = "ETR.ToDo";

    /// <summary>
    /// Default page size for paginated results
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum page size for paginated results
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Maximum number of task lists allowed for Basic users
    /// </summary>
    public const int BasicUserTaskListLimit = 10;

    /// <summary>
    /// Maximum number of tasks allowed for Basic users (across all lists)
    /// </summary>
    public const int BasicUserTaskLimit = 10;
}
