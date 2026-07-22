using Auth.Application.Abstractions.Repositories;
using Auth.Application.Abstractions.Services;
using Auth.Infrastructure.Caching;

using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Security.RefreshTokens.Initialization;

public sealed class RefreshTokenInitializationService(
    IRefreshTokenRepository refreshTokenRepository,
    RefreshTokenCacheService cacheService,
    ILogger<RefreshTokenInitializationService> logger) : IRefreshTokenInitializationService
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Initializing refresh token cache from database");

            var now = DateTime.UtcNow;
            var allTokens = await refreshTokenRepository.GetByUserIdAsync(Guid.Empty, ct);

            var activeTokens = allTokens
                .Where(t => t.ExpiryDate > now && !t.IsRevoked)
                .ToList();

            logger.LogInformation("Found {ActiveCount} active tokens out of {TotalCount}",
                activeTokens.Count, allTokens.Count);

            foreach (var token in activeTokens)
            {
                try
                {
                    await cacheService.SetAsync(token, RefreshTokenCacheService.TokenSyncStatus.Synced, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to cache token {JwtId} for user {UserId}",
                        token.JwtId, token.AccountId);
                }
            }

            logger.LogInformation("Refresh token cache initialization completed. Cached {Count} tokens",
                activeTokens.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Refresh token cache initialization failed");
            throw;
        }
    }
}
