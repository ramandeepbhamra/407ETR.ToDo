using System.Text;
using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Web.Core.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ETR.ToDo.Web.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Web.Core services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Web.Core infrastructure services
    /// </summary>
    public static IServiceCollection AddWebCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT options
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Register JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
        }

        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // preserve claim names exactly as issued in the token
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = JwtClaimTypes.Role
            };
        });

        return services;
    }
}
