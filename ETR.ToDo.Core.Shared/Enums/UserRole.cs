using System.Text.Json.Serialization;

namespace ETR.ToDo.Core.Shared.Enums;

/// <summary>
/// User role types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    /// <summary>Basic user with limited access (10 active task limit)</summary>
    Basic = 1,

    /// <summary>Premium user with unlimited tasks</summary>
    Premium = 2,

    /// <summary>Administrator — user/system management, API logs, and own tasks</summary>
    Admin = 3,

    /// <summary>Developer user with unlimited tasks and dev tool access</summary>
    Dev = 4
}
