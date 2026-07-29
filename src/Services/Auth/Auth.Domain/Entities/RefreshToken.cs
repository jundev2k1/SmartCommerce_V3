using BuildingBlock.Domain.Abstractions;

namespace Auth.Domain.Entities;

public sealed class RefreshToken : BaseEntity<Guid>
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = null!;
    public Guid JwtId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid jwtId, string token, DateTime expiryDate, Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            JwtId = jwtId,
            Token = token,
            ExpiryDate = expiryDate,
            AccountId = userId,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
