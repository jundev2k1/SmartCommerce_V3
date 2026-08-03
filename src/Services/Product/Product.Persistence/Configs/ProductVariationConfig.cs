using SmartEcommerce.BuildingBlock.Domain.Metadata;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SmartEcommerce.Product.Domain.Metadata;

namespace SmartEcommerce.Product.Persistence.Configs;

public sealed class ProductVariationConfig : IEntityTypeConfiguration<ProductVariation>
{
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        // Table
        builder.ToTable("product_variations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(x => x.Value, x => Sku.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Barcode)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : Barcode.Create(x))
            .HasMaxLength(14);

        builder.Property(x => x.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasColumnType("numeric(10,3)");

        builder.OwnsOne(x => x.Weight, weight =>
        {
            weight.Property(w => w.Value)
                .HasColumnName("weight")
                .HasColumnType("numeric(10,3)");

            weight.Property(w => w.Unit)
                .HasColumnName("weight_unit")
                .HasConversion<short>();
        });

        // Dimensions is a Value Object with no independent identity - mapped as an owned type
        // onto the same table (nested columns) since EF Core has no non-owned way to flatten a
        // nullable value object's members into sibling columns.
        builder.OwnsOne(x => x.Dimensions, dim =>
        {
            dim.Property(d => d.Length)
                .HasColumnName("dimensions_length")
                .HasColumnType("numeric(10,2)");
            dim.Property(d => d.Width)
                .HasColumnName("dimensions_width")
                .HasColumnType("numeric(10,2)");
            dim.Property(d => d.Height)
                .HasColumnName("dimensions_height")
                .HasColumnType("numeric(10,2)");
        });

        builder.Property(x => x.Images)
            .HasColumnType("jsonb");

        builder.Property(x => x.Status)
            .HasConversion<int>();
        builder.Property(x => x.IsDefault)
            .IsRequired();
        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ProductVariationMetadata>(x))
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Sku)
            .IsUnique();
        builder.HasIndex(x => x.ProductId);
    }
}
