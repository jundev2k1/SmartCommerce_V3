using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Product.Domain.ValueObjects;

namespace Product.Persistence.Configs;

public sealed class ProductTagConfig : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(
                x => x.Value,
                x => TagCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}
