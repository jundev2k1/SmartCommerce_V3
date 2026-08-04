using SmartEcommerce.Auth.Domain.Entities.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.Auth.Persistence.Configs;

public sealed class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.JwtId)
            .IsRequired()
            .HasComment("Access token JWT ID");

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(rt => rt.Account)
            .WithMany(a => a.RefreshTokens)
            .HasForeignKey(rt => rt.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.JwtId)
            .IsUnique()
            .HasDatabaseName("idx_refresh_tokens_jwt_id");

        builder.HasIndex(rt => rt.AccountId)
            .HasDatabaseName("idx_refresh_tokens_user_id");
    }
}
