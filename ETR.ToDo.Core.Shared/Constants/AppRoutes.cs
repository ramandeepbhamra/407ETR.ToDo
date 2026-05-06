namespace ETR.ToDo.Core.Shared.Constants;

/// <summary>
/// API route prefixes
/// </summary>
public static class AppRoutes
{
    public const string ApiPrefix = "api";
    public const string Auth = $"{ApiPrefix}/auth";
    public const string Users = $"{ApiPrefix}/users";
    public const string TaskLists = $"{ApiPrefix}/tasklists";
    public const string Tasks = $"{ApiPrefix}/tasks";
}
