using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Shared.Constants;
using ETR.ToDo.Services.Core.Authentication.Dto;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ETR.ToDo.Web.Core.Authentication;

/// <summary>
/// JWT token generation service
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    public TokenResponseDto GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtClaimTypes.UserId, user.Id.ToString()),
            new Claim(JwtClaimTypes.Email, user.Email),
            new Claim(JwtClaimTypes.FullName, user.FullName),
            new Claim(JwtClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new TokenResponseDto
        {
            AccessToken = tokenString,
            ExpiresAt = expiresAt,
            ExpiresInSeconds = _jwtOptions.ExpirationInMinutes * 60
        };
    }
}
