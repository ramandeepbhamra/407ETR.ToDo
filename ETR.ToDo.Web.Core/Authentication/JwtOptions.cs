namespace ETR.ToDo.Web.Core.Authentication;

/// <summary>
/// JWT configuration options
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Secret key for signing JWT tokens (should be at least 32 characters)
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (who created the token)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (who the token is intended for)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;
}
