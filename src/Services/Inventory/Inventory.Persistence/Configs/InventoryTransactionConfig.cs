using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configs;

public sealed class InventoryTransactionConfig : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InventoryId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductVariationId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.QuantityAfter)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.InventoryId);

        // Supports the transaction search's equality filters (see Task 5) - movements are
        // typically queried by product, variation, warehouse, or type, not just their own inventory row.
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductVariationId);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.Type);
    }
}
