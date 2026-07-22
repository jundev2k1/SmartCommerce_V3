using BuildingBlock.Domain.Metadata;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Product.Domain.Metadata;
using Product.Domain.ValueObjects;

namespace Product.Persistence.Configs;

public sealed class ProductConfig : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => ProductCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => Slug.Create(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ProductMetadata>(x))
            .HasColumnType("jsonb");

        ConfigureVariations(builder);
        ConfigureCategoryMappings(builder);
        ConfigureTagMappings(builder);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
    }

    // ProductVariation has no independent identity outside its Product (no repository, no
    // separate aggregate root) - same reasoning as Order/OrderItem - so it's mapped as an EF
    // owned collection; owned rows are always loaded with the Product, no explicit Include().
    private static void ConfigureVariations(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.OwnsMany(x => x.Variations, variation =>
        {
            variation.ToTable("product_variations");
            variation.WithOwner().HasForeignKey(v => v.ProductId);
            variation.HasKey(v => v.Id);

            variation.Property(v => v.Sku)
                .HasConversion(x => x.Value, x => Sku.Create(x))
                .HasMaxLength(50)
                .IsRequired();

            variation.Property(v => v.Barcode)
                .HasConversion(
                    x => x == null ? null : x.Value,
                    x => x == null ? null : Barcode.Create(x))
                .HasMaxLength(14);

            variation.Property(v => v.Price).HasColumnType("numeric(18,2)").IsRequired();
            variation.Property(v => v.Cost).HasColumnType("numeric(18,2)");
            variation.Property(v => v.Weight).HasColumnType("numeric(10,3)");

            // Dimensions is a nested owned type of the owned ProductVariation - EF's
            // ComplexProperty builder isn't available from an OwnedNavigationBuilder, so this
            // uses nested OwnsOne (mapped to the same product_variations table) instead.
            variation.OwnsOne(v => v.Dimensions, dim =>
            {
                dim.Property(d => d.Length).HasColumnName("dimensions_length").HasColumnType("numeric(10,2)");
                dim.Property(d => d.Width).HasColumnName("dimensions_width").HasColumnType("numeric(10,2)");
                dim.Property(d => d.Height).HasColumnName("dimensions_height").HasColumnType("numeric(10,2)");
            });

            variation.Property(v => v.Images).HasColumnType("jsonb");

            variation.Property(v => v.Status).HasConversion<int>();
            variation.Property(v => v.IsDefault).IsRequired();
            variation.Property(v => v.DisplayOrder).IsRequired();

            variation.Property(v => v.Metadata)
                .HasConversion(
                    x => x.ToJson(),
                    x => MetadataBase.FromJson<ProductVariationMetadata>(x))
                .HasColumnType("jsonb");

            variation.Property(v => v.CreatedAt).HasDefaultValueSql("now()");
            variation.Property(v => v.UpdatedAt).HasDefaultValueSql("now()");

            variation.HasIndex(v => v.Sku).IsUnique();
            variation.HasIndex(v => v.ProductId);
        });
    }

    // Product-ProductCategory / Product-ProductTag are many-to-many across independent
    // aggregate roots (Product doesn't own ProductCategory/ProductTag) - modeled as explicit
    // mapping entities (project style) rather than a raw id collection, and mapped as an EF
    // owned collection since a mapping row has no identity/lifecycle of its own outside Product.
    private static void ConfigureCategoryMappings(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.OwnsMany(x => x.CategoryMappings, mapping =>
        {
            mapping.ToTable("product_category_mappings");
            mapping.WithOwner().HasForeignKey(m => m.ProductId);
            mapping.HasKey(m => m.Id);

            mapping.Property(m => m.CategoryId).IsRequired();

            mapping.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
            mapping.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");

            mapping.HasIndex(m => new { m.ProductId, m.CategoryId }).IsUnique();
        });
    }

    private static void ConfigureTagMappings(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.OwnsMany(x => x.TagMappings, mapping =>
        {
            mapping.ToTable("product_tag_mappings");
            mapping.WithOwner().HasForeignKey(m => m.ProductId);
            mapping.HasKey(m => m.Id);

            mapping.Property(m => m.TagId).IsRequired();

            mapping.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
            mapping.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");

            mapping.HasIndex(m => new { m.ProductId, m.TagId }).IsUnique();
        });
    }
}
