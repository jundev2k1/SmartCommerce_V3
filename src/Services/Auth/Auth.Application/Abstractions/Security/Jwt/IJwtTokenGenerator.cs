using System.Security.Claims;

namespace SmartEcommerce.Auth.Application.Abstractions.Security.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, string username, IEnumerable<string> roles, Guid? jwtId = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);
}
