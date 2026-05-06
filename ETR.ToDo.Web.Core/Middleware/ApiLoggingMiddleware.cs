using System.Diagnostics;
using ETR.ToDo.Core.Entities.Logging;
using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Services.Core.Logging;
using ETR.ToDo.Services.Core.Logging.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ETR.ToDo.Web.Core.Middleware;

/// <summary>
/// Records every API request as an ApiLog entry in the database.
/// Uses IServiceScopeFactory because middleware is singleton but IApiLogService is scoped.
/// </summary>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    private static readonly HashSet<string> SkippedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/swagger", "/scalar", "/openapi", "/favicon.ico"
    };

    public ApiLoggingMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var executionTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        string? exceptionMessage = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exceptionMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            await SaveLogAsync(context, executionTime, stopwatch.ElapsedMilliseconds, exceptionMessage);
        }
    }

    private async Task SaveLogAsync(
        HttpContext context,
        DateTime executionTime,
        long durationMs,
        string? exceptionMessage)
    {
        try
        {
            var routeData = context.GetRouteData();
            var serviceName = routeData?.Values["controller"]?.ToString() ?? context.Request.Path.ToString();
            var methodName = routeData?.Values["action"]?.ToString();

            var rawParams = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null;
            var parameters = rawParams?.Length > ApiLog.MaxParametersLength
                ? rawParams[..ApiLog.MaxParametersLength]
                : rawParams;

            Guid? userId = null;
            string? userName = null;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var raw = context.User.FindFirst(JwtClaimTypes.UserId)?.Value;
                if (Guid.TryParse(raw, out var parsed))
                    userId = parsed;

                userName = context.User.FindFirst(JwtClaimTypes.Email)?.Value;
            }

            var rawAgent = context.Request.Headers.UserAgent.ToString();
            var userAgent = rawAgent.Length > ApiLog.MaxUserAgentLength
                ? rawAgent[..ApiLog.MaxUserAgentLength]
                : rawAgent;

            var rawException = exceptionMessage;
            var truncatedException = rawException?.Length > ApiLog.MaxExceptionMessageLength
                ? rawException[..ApiLog.MaxExceptionMessageLength]
                : rawException;

            var dto = new CreateApiLogDto
            {
                UserId = userId,
                UserName = userName,
                HttpMethod = context.Request.Method,
                ServiceName = serviceName,
                MethodName = methodName,
                Parameters = parameters,
                ExecutionTime = executionTime,
                ExecutionDuration = (int)durationMs,
                ClientIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent,
                StatusCode = exceptionMessage != null ? 500 : context.Response.StatusCode,
                ExceptionMessage = truncatedException
            };

            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IApiLogService>();
            await service.CreateAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save API log entry");
        }
    }

    private static bool ShouldSkip(PathString path)
    {
        return SkippedPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }
}
