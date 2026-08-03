using SmartEcommerce.Auth.Application.Abstractions.Services;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;

namespace SmartEcommerce.Auth.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler(
    ICurrentUserService currentUserService,
    IRefreshTokenService refreshTokenService) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct = default)
    {
        var refreshToken = currentUserService.GetRefreshToken();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await refreshTokenService.RevokeRefreshTokenByTokenStringAsync(refreshToken, ct);
        }

        currentUserService.RemoveAccessToken();
        currentUserService.RemoveRefreshToken();
    }
}
