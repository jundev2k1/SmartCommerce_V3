using Auth.Application.Abstractions.Auth;
using Auth.Application.Abstractions.Security.Jwt;
using Auth.Application.Abstractions.Services;

namespace Auth.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    IAuthService authService,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct = default)
    {
        var isValid = await authService.ValidateCredentialsAsync(request.Email, request.Password, ct);
        if (!isValid)
            throw new UnauthorizedException("Invalid credentials");

        var user = await authService.GetUserByEmailAsync(request.Email, ct)
            ?? throw new NotFoundException("User", request.Email);

        var jwtId = Guid.NewGuid();
        var roles = await authService.GetUserRolesAsync(user.Id, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.UserName!,
            roles,
            jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        return new LoginResult(accessToken, refreshToken);
    }
}
