using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartEcommerce.Product.Persistence.Configs;

public sealed class ProductTagConfig : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        // Table
        builder.ToTable("product_tags");

        // Properties
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

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}
