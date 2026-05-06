namespace ETR.ToDo.Core.Shared.Constants;

/// <summary>
/// Validation length constants shared across entities and DTOs.
/// Single source of truth — change here to apply everywhere.
/// </summary>
public static class ValidationConstants
{
    /// <summary>Validation lengths for task and subtask titles.</summary>
    public static class TaskTitle
    {
        public const int MinLength = 1;
        public const int MaxLength = 255;
    }

    /// <summary>Validation lengths for task list titles.</summary>
    public static class TaskListTitle
    {
        public const int MinLength = 1;
        public const int MaxLength = 100;
    }

    /// <summary>Validation lengths for user name fields (FirstName, LastName).</summary>
    public static class UserName
    {
        public const int MinLength = 1;
        public const int MaxLength = 100;
    }

    /// <summary>Validation lengths for email addresses.</summary>
    public static class Email
    {
        public const int MaxLength = 256;
    }

    /// <summary>Validation lengths for passwords (plain text — not the stored hash).</summary>
    public static class Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 100;
    }

    /// <summary>Validation lengths for password hashes (BCrypt output).</summary>
    public static class PasswordHash
    {
        public const int MaxLength = 256;
    }

    /// <summary>Validation lengths for phone numbers.</summary>
    public static class PhoneNumber
    {
        public const int MaxLength = 20;
    }
}
