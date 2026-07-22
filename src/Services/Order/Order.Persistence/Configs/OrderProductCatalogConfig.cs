using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Persistence.Configs;

public sealed class OrderProductCatalogConfig : IEntityTypeConfiguration<OrderProductCatalog>
{
    public void Configure(EntityTypeBuilder<OrderProductCatalog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => x.ProductId);
    }
}
