using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configs;

public sealed class InventoryConfig : IEntityTypeConfiguration<InventoryEntity>
{
    public void Configure(EntityTypeBuilder<InventoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Available)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.VariantId, x.WarehouseId })
            .IsUnique();

        builder.HasIndex(x => x.ProductId);
        
        builder.HasIndex(x => x.WarehouseId);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
