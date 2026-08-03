using SmartEcommerce.BuildingBlock.Domain.Metadata;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SmartEcommerce.Product.Domain.Metadata;

namespace SmartEcommerce.Product.Persistence.Configs;

public sealed class ProductConfig : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        // Table
        builder.ToTable("products");

        // Properties
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

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Relationships
        builder.HasMany(x => x.Variations)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CategoryMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TagMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
