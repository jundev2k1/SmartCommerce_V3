using Auth.Application.Abstractions.Auth;
using Auth.Application.Abstractions.Security.Jwt;
using Auth.Application.Abstractions.Services;

using BuildingBlock.Domain.Enums;
using BuildingBlock.SharedKernel.Extensions;

namespace Auth.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler(
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IAuthService authService,
    ICurrentUserService currentUserService) : ICommandHandler<RefreshTokenCommand>
{
    public async Task Handle(RefreshTokenCommand request, CancellationToken ct = default)
    {
        var refreshToken = currentUserService.GetRefreshToken();
        if (refreshToken.IsNullOrWhiteSpace())
            throw new UnauthorizedException("Refresh token not found in cookies");

        var (userId, isValid) = await refreshTokenService.ValidateAndGetUserIdAsync(refreshToken, ct);
        if (!isValid)
            throw new UnauthorizedException(MessageCode.InvalidToken);

        var user = await authService.GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        var jwtId = Guid.NewGuid();
        var roles = await authService.GetUserRolesAsync(user.Id, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email!,
            user.UserName!,
            roles,
            jwtId);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(newRefreshToken);
    }
}
