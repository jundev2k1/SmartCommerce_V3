using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.Persistence.Configs;

public sealed class ProductCategoryConfig : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        // Table
        builder.ToTable("product_categories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => CategoryCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Relationships
        // Self-referencing hierarchy - no Domain-level Parent/Children navigation (see
        // ProductCategory.ChangeParent remarks), so this is a shadow-navigation FK only.
        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ParentCategoryId);
    }
}
