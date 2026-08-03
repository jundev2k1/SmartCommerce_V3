using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.User.Persistence.Configs;

public sealed class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PhoneSearch)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PhoneReverse)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.Roles)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PhoneSearch);
        builder.HasIndex(x => x.PhoneReverse);

        // GIN index for array-containment queries (role filter uses `Roles.Contains(x)`, which
        // Npgsql translates to `@>`/`= ANY`) - a btree index (the default) can't serve those.
        builder.HasIndex(x => x.Roles).HasMethod("gin");
    }
}
