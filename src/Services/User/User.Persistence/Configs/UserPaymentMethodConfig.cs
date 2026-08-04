using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.User.Persistence.Configs;

public sealed class UserPaymentMethodConfig : IEntityTypeConfiguration<UserPaymentMethod>
{
    public void Configure(EntityTypeBuilder<UserPaymentMethod> builder)
    {
        // Table
        builder.ToTable("user_payment_methods");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.PaymentType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.ExternalCustomerId)
            .HasMaxLength(256);

        builder.Property(x => x.ExternalPaymentMethodId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(x => x.CardInformation, card =>
        {
            card.Property(c => c.CardHolderName)
                .HasColumnName("card_holder_name")
                .HasMaxLength(200);

            card.Property(c => c.MaskedNumber)
                .HasColumnName("card_masked_number")
                .HasMaxLength(30);

            card.Property(c => c.Last4Digits)
                .HasColumnName("card_last4_digits")
                .HasMaxLength(4);

            card.Property(c => c.ExpireMonth)
                .HasColumnName("card_expire_month");

            card.Property(c => c.ExpireYear)
                .HasColumnName("card_expire_year");

            card.Property(c => c.Brand)
                .HasColumnName("card_brand")
                .HasConversion<short>();
        });

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDefault });
        builder.HasIndex(x => new { x.Provider, x.ExternalPaymentMethodId })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
