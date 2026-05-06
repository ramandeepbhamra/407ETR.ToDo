using ETR.ToDo.Web.Core.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ETR.ToDo.Web.Core.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder to configure middleware pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds global exception handling middleware
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Adds API request logging middleware — records every hit to the ApiLogs table
    /// </summary>
    public static IApplicationBuilder UseApiLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiLoggingMiddleware>();
    }
}
