using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configs;

public sealed class InventoryConfig : IEntityTypeConfiguration<InventoryEntity>
{
    public void Configure(EntityTypeBuilder<InventoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductVariationId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        // Stock-keeping unit is (ProductVariationId, WarehouseId) now - a variation is where
        // physical stock actually lives, since Product can no longer exist without one. ProductId
        // stays indexed (non-unique) purely so "total stock for a product" can filter without a
        // cross-service join back to Product.
        builder.HasIndex(x => new { x.ProductVariationId, x.WarehouseId }).IsUnique();
        builder.HasIndex(x => x.ProductId);

        // Postgres system column, not a domain field - guards concurrent Decrease/Increase/Adjust
        // calls against the same row (StockOut vs DeductStock vs Adjust racing) so the loser gets
        // a conflict to retry against instead of silently overwriting the winner's write. See
        // EfUnitOfWork.ExecuteTransactionAsync, which translates the resulting
        // DbUpdateConcurrencyException into an Application-layer ConflictException.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
